using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Agent;

public partial class MainWindow : Window
{
    private readonly IInstalledSoftwareScanner _scanner;
    private readonly IUpdateSynchronizationService _synchronizationService;
    private readonly IUpdateDownloadService _updateDownloadService;
    private readonly ObservableCollection<InstalledApplication> _applications = [];
    private readonly ObservableCollection<UpdateRow> _updates = [];
    private readonly ICollectionView _applicationsView;
    private CancellationTokenSource? _operationCancellationTokenSource;

    public MainWindow(
        IInstalledSoftwareScanner scanner,
        IUpdateSynchronizationService synchronizationService,
        IUpdateDownloadService updateDownloadService,
        ISystemArchitectureDetector architectureDetector)
    {
        InitializeComponent();
        _scanner = scanner;
        _synchronizationService = synchronizationService;
        _updateDownloadService = updateDownloadService;
        ArchitectureText.Text = architectureDetector.Detect();
        ApplicationsGrid.ItemsSource = _applications;
        UpdatesGrid.ItemsSource = _updates;
        _applicationsView = CollectionViewSource.GetDefaultView(_applications);
        _applicationsView.Filter = FilterApplication;
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        BeginOperation("Scanning the Windows registry...");

        try
        {
            var applications = await _scanner.ScanAsync(
                _operationCancellationTokenSource!.Token);

            _applications.Clear();
            foreach (var application in applications)
                _applications.Add(application);

            ClearUpdates();
            RefreshView();
            StatusText.Text = $"Scan completed. Found {applications.Count} applications.";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Operation canceled.";
        }
        catch (Exception exception)
        {
            ShowError("The scan failed.", exception);
        }
        finally
        {
            EndOperation();
        }
    }

    private async void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
    {
        BeginOperation("Connecting to the SmartInstaller API...");

        try
        {
            var result = await _synchronizationService.CheckUpdatesAsync(
                _applications.ToArray(),
                _operationCancellationTokenSource!.Token);

            ClearUpdates();

            foreach (var item in result.Items)
            {
                var row = new UpdateRow(item);
                row.PropertyChanged += UpdateRow_PropertyChanged;
                _updates.Add(row);
            }

            UpdatesTab.Header = $"Updates ({result.UpdateCount})";
            StatusText.Text = result.MatchedApplicationCount == 0
                ? "No installed applications matched the current SmartInstaller catalog."
                : $"Matched {result.MatchedApplicationCount} applications. Found {result.UpdateCount} updates.";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Operation canceled.";
        }
        catch (HttpRequestException exception)
        {
            ShowError(
                "Could not connect to the SmartInstaller API. Make sure the API is running and the URL in appsettings.json is correct.",
                exception);
        }
        catch (Exception exception)
        {
            ShowError("The update check failed.", exception);
        }
        finally
        {
            EndOperation();
        }
    }

    private async void DownloadSelectedButton_Click(object sender, RoutedEventArgs e)
    {
        var selected = _updates
            .Where(row => row.IsSelected && row.CanDownload)
            .ToArray();

        if (selected.Length == 0)
        {
            StatusText.Text = "Select at least one available update.";
            return;
        }

        BeginOperation($"Downloading {selected.Length} update(s)...");

        try
        {
            var completed = 0;

            foreach (var row in selected)
            {
                var token = _operationCancellationTokenSource!.Token;
                token.ThrowIfCancellationRequested();

                row.Status = "Preparing download";
                row.Percentage = 0;

                var progress = new Progress<DownloadProgress>(value =>
                {
                    row.Percentage = value.Percentage ?? 0;
                    row.Status = "Downloading";
                    StatusText.Text = $"Downloading {row.ApplicationName}: {row.ProgressText}";
                });

                var result = await _updateDownloadService.DownloadAsync(
                    row.Update,
                    progress,
                    token);

                row.FilePath = result.DownloadResult.FilePath;
                row.IsSelected = false;

                row.Status = result.DownloadResult.Status switch
                {
                    DownloadStatus.Completed => "Downloaded and verified",
                    DownloadStatus.Cached => "Ready from cache",
                    DownloadStatus.Cancelled => "Canceled",
                    DownloadStatus.VerificationFailed => "Verification failed",
                    _ => result.DownloadResult.ErrorMessage ?? "Download failed"
                };

                if (result.DownloadResult.IsSuccess)
                {
                    row.Percentage = 100;
                    completed++;
                }
            }

            StatusText.Text = $"Download operation completed. {completed}/{selected.Length} installer(s) ready.";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Download operation canceled.";
        }
        catch (HttpRequestException exception)
        {
            ShowError("Could not retrieve the installer manifest.", exception);
        }
        catch (Exception exception)
        {
            ShowError("The download operation failed.", exception);
        }
        finally
        {
            EndOperation();
        }
    }

    private void ClearUpdates()
    {
        foreach (var row in _updates)
            row.PropertyChanged -= UpdateRow_PropertyChanged;

        _updates.Clear();
        UpdatesTab.Header = "Updates (0)";
    }

    private void UpdateRow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(UpdateRow.IsSelected) or nameof(UpdateRow.Status))
            RefreshDownloadButton();
    }

    private void BeginOperation(string status)
    {
        _operationCancellationTokenSource?.Dispose();
        _operationCancellationTokenSource = new CancellationTokenSource();
        SetBusyState(true);
        StatusText.Text = status;
    }

    private void EndOperation() => SetBusyState(false);

    private void CancelButton_Click(object sender, RoutedEventArgs e) =>
        _operationCancellationTokenSource?.Cancel();

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
        RefreshView();

    private bool FilterApplication(object item)
    {
        if (item is not InstalledApplication application)
            return false;

        var search = SearchTextBox.Text.Trim();
        return string.IsNullOrWhiteSpace(search) ||
               Contains(application.Name, search) ||
               Contains(application.Version, search) ||
               Contains(application.Publisher, search);
    }

    private static bool Contains(string? value, string search) =>
        value?.Contains(search, StringComparison.CurrentCultureIgnoreCase) == true;

    private void RefreshView()
    {
        _applicationsView.Refresh();
        CountText.Text = $"{_applicationsView.Cast<object>().Count()} applications";
    }

    private void SetBusyState(bool isBusy)
    {
        ScanButton.IsEnabled = !isBusy;
        CheckUpdatesButton.IsEnabled = !isBusy && _applications.Count > 0;
        CancelButton.IsEnabled = isBusy;
        SearchTextBox.IsEnabled = !isBusy;
        UpdatesGrid.IsEnabled = !isBusy;
        RefreshDownloadButton(isBusy);
    }

    private void RefreshDownloadButton(bool isBusy = false)
    {
        DownloadSelectedButton.IsEnabled =
            !isBusy && _updates.Any(row => row.IsSelected && row.CanDownload);
    }

    private void ShowError(string message, Exception exception)
    {
        StatusText.Text = message;
        MessageBox.Show(
            this,
            $"{message}\n\n{exception.Message}",
            "SmartInstaller Agent",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void ApplicationsGrid_Sorting(object sender, DataGridSortingEventArgs e) =>
        StatusText.Text = $"Sorted by {e.Column.Header}.";

    protected override void OnClosed(EventArgs e)
    {
        _operationCancellationTokenSource?.Cancel();
        _operationCancellationTokenSource?.Dispose();
        base.OnClosed(e);
    }

    private sealed class UpdateRow : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _status;
        private double _percentage;
        private string? _filePath;

        public UpdateRow(UpdateCheckItem update)
        {
            Update = update;
            _isSelected = update.UpdateAvailable && update.InstallerProfileId.HasValue;
            _status = update.UpdateAvailable
                ? update.InstallerProfileId.HasValue
                    ? "Update available"
                    : "No compatible installer"
                : "Up to date";
        }

        public UpdateCheckItem Update { get; }
        public string ApplicationName => Update.ApplicationName;
        public string InstalledVersion => Update.InstalledVersion;
        public string LatestVersion => Update.LatestVersion;
        public bool CanDownload => Update.UpdateAvailable && Update.InstallerProfileId.HasValue;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!CanDownload) value = false;
                SetField(ref _isSelected, value);
            }
        }

        public string Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        public double Percentage
        {
            get => _percentage;
            set
            {
                if (SetField(ref _percentage, value))
                    OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string ProgressText => $"{Percentage:0}%";

        public string? FilePath
        {
            get => _filePath;
            set => SetField(ref _filePath, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
