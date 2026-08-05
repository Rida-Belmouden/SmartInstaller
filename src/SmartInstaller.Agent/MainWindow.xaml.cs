using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Session;
using SmartInstaller.Agent.Core.Installation.Models;
using SmartInstaller.Agent.Core.Installation.Verification;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;
using SmartInstaller.Agent.Core.Installation.Services;

namespace SmartInstaller.Agent;

public partial class MainWindow : Window
{
    private readonly IInstalledSoftwareScanner _scanner;
    private readonly IUpdateSynchronizationService _synchronizationService;
    private readonly IDownloadSessionController _downloadSessionController;
    private readonly IUpdateDownloadStateService _updateDownloadStateService;
    private readonly IUpdateInstallationService _updateInstallationService;
    private readonly ObservableCollection<InstalledApplication> _applications = [];
    private readonly ObservableCollection<UpdateRow> _updates = [];
    private readonly ICollectionView _applicationsView;    

    private CancellationTokenSource? _operationCancellationTokenSource;
    private bool _isBusy;

    public MainWindow(
        IInstalledSoftwareScanner scanner,
        IUpdateSynchronizationService synchronizationService,
        IDownloadSessionController downloadSessionController,
        IUpdateDownloadStateService updateDownloadStateService,
        IUpdateInstallationService updateInstallationService,
        ISystemArchitectureDetector architectureDetector)
    {
        InitializeComponent();

        _scanner = scanner;
        _synchronizationService = synchronizationService;
        _downloadSessionController = downloadSessionController;
        _updateDownloadStateService = updateDownloadStateService;
        _updateInstallationService = updateInstallationService;

        ArchitectureText.Text = architectureDetector.Detect();
        ApplicationsGrid.ItemsSource = _applications;
        UpdatesGrid.ItemsSource = _updates;

        _applicationsView =
            CollectionViewSource.GetDefaultView(_applications);

        _applicationsView.Filter = FilterApplication;

        _downloadSessionController.SessionEvent +=
            DownloadSessionController_SessionEvent;
    }

    private async void ScanButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        BeginOperation("Scanning the Windows registry...");

        try
        {
            var applications = await _scanner.ScanAsync(
                CurrentToken);

            _applications.Clear();

            foreach (var application in applications)
            {
                _applications.Add(application);
            }

            ClearUpdates();
            RefreshView();

            StatusText.Text =
                $"Scan completed. Found {applications.Count} applications.";
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

    private async void CheckUpdatesButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        BeginOperation(
            "Connecting to the SmartInstaller API...");

        try
        {
            var result =
                await _synchronizationService.CheckUpdatesAsync(
                    _applications.ToArray(),
                    CurrentToken);

            ClearUpdates();

            foreach (var item in result.Items)
            {
                var row = new UpdateRow(item);

                row.PropertyChanged +=
                    UpdateRow_PropertyChanged;

                _updates.Add(row);
            }

            await RefreshDownloadStatesAsync(
                CurrentToken);

            UpdatesTab.Header =
                $"Updates ({result.UpdateCount})";

            StatusText.Text =
                result.MatchedApplicationCount == 0
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
                "Could not connect to the SmartInstaller API. Make sure the API is running and appsettings.json is correct.",
                exception);
        }
        catch (Exception exception)
        {
            ShowError(
                "The update check failed.",
                exception);
        }
        finally
        {
            EndOperation();
        }
    }

    private async void DownloadSelectedButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var selected = _updates
            .Where(row =>
                row.IsSelected &&
                row.CanDownload)
            .ToArray();

        if (selected.Length == 0)
        {
            StatusText.Text =
                "Select at least one available update.";
            return;
        }

        BeginOperation(
            $"Downloading {selected.Length} update(s) concurrently...");

        foreach (var row in selected)
        {
            var wasPaused =
                row.HasPartialDownload;

            row.IsDownloading = true;
            row.Status = "Queued";

            if (!wasPaused)
            {
                row.Percentage = 0;
                row.InitialPartialBytes = 0;
            }
        }

        try
        {
            await _downloadSessionController.StartAsync(
                selected
                    .Select(row => row.Update)
                    .ToArray(),
                CurrentToken);
        }
        catch (OperationCanceledException)
        {
            StatusText.Text =
                "Concurrent download operation canceled.";
        }
        catch (Exception exception)
        {
            ShowError(
                "The concurrent download operation failed.",
                exception);
        }
        finally
        {
            foreach (var row in selected)
            {
                row.IsDownloading = false;
                row.NotifyAvailabilityChanged();
            }

            EndOperation();
        }
    }

    private void DownloadSessionController_SessionEvent(
        object? sender,
        DownloadSessionEvent sessionEvent)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(
                () => DownloadSessionController_SessionEvent(
                    sender,
                    sessionEvent));
            return;
        }

        if (sessionEvent.Item is not null)
        {
            var item = sessionEvent.Item;
            var row = _updates.FirstOrDefault(candidate =>
                candidate.Update.ApplicationId ==
                    item.Update.ApplicationId &&
                candidate.Update.InstallerProfileId ==
                    item.Update.InstallerProfileId);

            if (row is null)
            {
                return;
            }

            var isResuming =
                row.HasPartialDownload ||
                row.InitialPartialBytes > 0;

            row.Status = item.Status switch
            {
                "Starting" when isResuming =>
                    "Preparing to resume",
                "Downloading" when isResuming =>
                    "Resuming",
                _ => item.Status
            };
            row.Percentage =
                item.Percentage > 0
                    ? item.Percentage
                    : row.Percentage;
            row.IsDownloading = item.IsDownloading;
            if (!item.IsDownloading)
            {
                row.HasPartialDownload =
                    item.HasPartialDownload;
                row.InitialPartialBytes =
                    item.InitialPartialBytes;
            }
            row.Manifest = item.Manifest ?? row.Manifest;
            row.FilePath = item.FilePath;
            if (!item.IsDownloading)
            {
                row.IsSelected =
                    item.HasPartialDownload ||
                    item.Percentage >= 100;
            }
            row.NotifyAvailabilityChanged();

            if (item.IsDownloading &&
                item.Status is "Downloading")
            {
                StatusText.Text =
                    $"{row.Status} {row.ApplicationName}: " +
                    $"{row.ProgressText}";
            }

            return;
        }

        StatusText.Text = sessionEvent.Type switch
        {
            DownloadSessionEventType.SessionStarted =>
                sessionEvent.Message ?? "Download session started.",
            DownloadSessionEventType.SessionCompleted =>
                $"Concurrent downloads completed. {sessionEvent.Message}",
            DownloadSessionEventType.SessionCancelled =>
                sessionEvent.Message ?? "Download session canceled.",
            DownloadSessionEventType.SessionFailed =>
                $"Download session failed. {sessionEvent.Message}",
            _ => StatusText.Text
        };
    }

    private async void InstallSelectedButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var selected = _updates
            .Where(row =>
                row.IsSelected &&
                row.CanInstall)
            .ToArray();

        if (selected.Length == 0)
        {
            StatusText.Text =
                "Select at least one downloaded installer.";
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            $"Install {selected.Length} selected update(s) silently?",
            "SmartInstaller Agent",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        BeginOperation(
            $"Installing {selected.Length} update(s)...");

        try
        {
            var succeeded = 0;

            foreach (var row in selected)
            {
                CurrentToken.ThrowIfCancellationRequested();

                row.Status = "Launching installer";
                StatusText.Text =
                    $"Installing {row.ApplicationName}...";

                var result =
                    await _updateInstallationService.InstallAsync(
                        row.Update,
                        row.Manifest!,
                        row.FilePath!,
                        CurrentToken);

                row.Status =
                    GetInstallationStatus(result);

                if (result.VerificationResult.IsVerified)
                {
                    row.InstalledVersion =
                        result.VerificationResult.DetectedVersion
                        ?? row.LatestVersion;

                    row.IsInstalled = true;
                    row.IsSelected = false;
                    row.Percentage = 100;
                    succeeded++;
                }
                else if (result.InstallResult.Status ==
                         InstallStatus.RestartRequired)
                {
                    row.IsSelected = false;
                    succeeded++;
                }

                row.NotifyAvailabilityChanged();
            }

            await RefreshInstalledApplicationsAsync(
                CurrentToken);

            var completedRows = selected
                .Where(row => row.IsInstalled)
                .ToArray();

            foreach (var row in completedRows)
            {
                row.PropertyChanged -=
                    UpdateRow_PropertyChanged;

                _updates.Remove(row);
            }

            UpdatesTab.Header =
                $"Updates ({_updates.Count(row => !row.IsInstalled)})";

            StatusText.Text =
                $"Installation completed. {succeeded}/{selected.Length} update(s) installed and verified.";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text =
                "Installation operation canceled.";
        }
        catch (Exception exception)
        {
            ShowError(
                "The installation operation failed.",
                exception);
        }
        finally
        {
            EndOperation();
        }
    }

    private static string GetInstallationStatus(
        UpdateInstallationResult result)
    {
        if (!result.InstallResult.IsSuccess)
        {
            return result.InstallResult.Status switch
            {
                InstallStatus.Cancelled =>
                    "Installation canceled",

                InstallStatus.TimedOut =>
                    "Installation timed out",

                InstallStatus.FileNotFound =>
                    "Installer file not found",

                InstallStatus.UnsupportedInstaller =>
                    "Unsupported installer",

                _ =>
                    result.InstallResult.ErrorMessage ??
                    "Installation failed"
            };
        }

        return result.VerificationResult.Status switch
        {
            InstallationVerificationStatus.Verified =>
                "Installed and verified",

            InstallationVerificationStatus.PendingRestart =>
                "Installed - restart required",

            InstallationVerificationStatus.ApplicationNotFound =>
                "Installed, but application was not detected",

            InstallationVerificationStatus.VersionUnavailable =>
                "Installed, but version could not be verified",

            InstallationVerificationStatus.VersionMismatch =>
                result.VerificationResult.Message ??
                "Installed, but version mismatch",

            _ =>
                "Installed, verification not required"
        };
    }

    private async Task RefreshInstalledApplicationsAsync(
        CancellationToken cancellationToken)
    {
        var applications =
            await _scanner.ScanAsync(cancellationToken);

        _applications.Clear();

        foreach (var application in applications)
        {
            _applications.Add(application);
        }

        RefreshView();
    }

    private async Task RefreshDownloadStatesAsync(
        CancellationToken cancellationToken)
    {
        foreach (var row in _updates)
        {
            await RefreshDownloadStateAsync(
                row,
                cancellationToken);
        }

        RefreshActionButtons();
    }

    private async Task RefreshDownloadStateAsync(
        UpdateRow row,
        CancellationToken cancellationToken)
    {
        var state =
            await _updateDownloadStateService.GetStateAsync(
                row.Update,
                cancellationToken);

        if (state is null)
        {
            return;
        }

        row.Manifest = state.Manifest;

        if (state.FinalFileExists)
        {
            row.FilePath = state.FinalPath;
            row.Percentage = 100;
            row.HasPartialDownload = false;
            row.Status = "Ready from cache";
            return;
        }

        row.FilePath = null;
        row.InitialPartialBytes =
            state.PartialBytes;

        row.HasPartialDownload =
            state.HasPartialFile;

        row.IsDownloading = false;

        if (state.HasPartialFile)
        {
            row.Percentage =
                state.Percentage;

            row.Status = "Paused";
            row.IsSelected = true;
        }
    }

    private CancellationToken CurrentToken =>
        _operationCancellationTokenSource?.Token ??
        CancellationToken.None;

    private void ClearUpdates()
    {
        foreach (var row in _updates)
        {
            row.PropertyChanged -=
                UpdateRow_PropertyChanged;
        }

        _updates.Clear();
        UpdatesTab.Header = "Updates (0)";
    }

    private void UpdateRow_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName is
            nameof(UpdateRow.IsSelected) or
            nameof(UpdateRow.Status) or
            nameof(UpdateRow.FilePath) or
            nameof(UpdateRow.Manifest) or
            nameof(UpdateRow.IsInstalled) or
            nameof(UpdateRow.HasPartialDownload) or
            nameof(UpdateRow.IsDownloading))
        {
            RefreshActionButtons();
        }
    }

    private void BeginOperation(string status)
    {
        _operationCancellationTokenSource?.Dispose();

        _operationCancellationTokenSource =
            new CancellationTokenSource();

        _isBusy = true;
        SetBusyState(true);
        StatusText.Text = status;
    }

    private void EndOperation()
    {
        _isBusy = false;
        SetBusyState(false);
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _downloadSessionController.CancelAll();
        _operationCancellationTokenSource?.Cancel();
    }

    private void SearchTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        RefreshView();
    }

    private bool FilterApplication(object item)
    {
        if (item is not InstalledApplication application)
        {
            return false;
        }

        var search = SearchTextBox.Text.Trim();

        return string.IsNullOrWhiteSpace(search) ||
               Contains(application.Name, search) ||
               Contains(application.Version, search) ||
               Contains(application.Publisher, search);
    }

    private static bool Contains(
        string? value,
        string search)
    {
        return value?.Contains(
            search,
            StringComparison.CurrentCultureIgnoreCase) ==
            true;
    }

    private void RefreshView()
    {
        _applicationsView.Refresh();

        CountText.Text =
            $"{_applicationsView.Cast<object>().Count()} applications";
    }

    private void SetBusyState(bool isBusy)
    {
        ScanButton.IsEnabled = !isBusy;

        CheckUpdatesButton.IsEnabled =
            !isBusy &&
            _applications.Count > 0;

        CancelButton.IsEnabled = isBusy;
        SearchTextBox.IsEnabled = !isBusy;
        UpdatesGrid.IsEnabled = !isBusy;

        RefreshActionButtons();
    }

    private void RefreshActionButtons()
    {
        var selectedDownloads =
            _updates.Where(row =>
                row.IsSelected &&
                row.CanDownload)
            .ToArray();

        DownloadSelectedButton.IsEnabled =
            !_isBusy &&
            selectedDownloads.Length > 0;

        DownloadSelectedButton.Content =
            selectedDownloads.Length > 0 &&
            selectedDownloads.All(row =>
                row.CanResume)
                ? "Resume selected"
                : selectedDownloads.Any(row =>
                    row.CanResume)
                    ? "Download / resume"
                    : "Download selected";

        InstallSelectedButton.IsEnabled =
                !_isBusy &&
                _updates.Any(row =>
                    row.IsSelected &&
                    row.CanInstall);
    }

    private void ShowError(
        string message,
        Exception exception)
    {
        StatusText.Text = message;

        MessageBox.Show(
            this,
            $"{message}\n\n{exception.Message}",
            "SmartInstaller Agent",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void ApplicationsGrid_Sorting(
        object sender,
        DataGridSortingEventArgs e)
    {
        StatusText.Text =
            $"Sorted by {e.Column.Header}.";
    }

    protected override void OnClosed(EventArgs e)
    {
        _downloadSessionController.SessionEvent -=
            DownloadSessionController_SessionEvent;
        _downloadSessionController.CancelAll();
        _operationCancellationTokenSource?.Cancel();
        _operationCancellationTokenSource?.Dispose();

        base.OnClosed(e);
    }

    private sealed class UpdateRow
        : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _status;
        private double _percentage;
        private string? _filePath;
        private InstallerManifest? _manifest;
        private bool _isInstalled;
        private bool _hasPartialDownload;
        private bool _isDownloading;
        private long _initialPartialBytes;

        public UpdateRow(UpdateCheckItem update)
        {
            Update = update;

            _installedVersion = update.InstalledVersion;

            _isSelected =
                update.UpdateAvailable &&
                update.InstallerProfileId.HasValue;

            _status = update.UpdateAvailable
                ? update.InstallerProfileId.HasValue
                    ? "Update available"
                    : "No compatible installer"
                : "Up to date";
        }

        public UpdateCheckItem Update { get; }

        public string ApplicationName =>
            Update.ApplicationName;

        private string _installedVersion;

        public string InstalledVersion
        {
            get => _installedVersion;
            set => SetField(
                ref _installedVersion,
                value);
        }

        public string LatestVersion =>
            Update.LatestVersion;

        public bool CanDownload =>
            !_isInstalled &&
            !IsDownloading &&
            Update.UpdateAvailable &&
            Update.InstallerProfileId.HasValue &&
            string.IsNullOrWhiteSpace(FilePath);

        public bool CanResume =>
            CanDownload &&
            HasPartialDownload;

        public bool CanInstall =>
            !_isInstalled &&
            Manifest is not null &&
            !string.IsNullOrWhiteSpace(FilePath) &&
            File.Exists(FilePath);

        public bool HasPartialDownload
        {
            get => _hasPartialDownload;
            set
            {
                if (SetField(
                        ref _hasPartialDownload,
                        value))
                {
                    OnPropertyChanged(
                        nameof(CanResume));
                    OnPropertyChanged(
                        nameof(CanDownload));
                }
            }
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                if (SetField(
                        ref _isDownloading,
                        value))
                {
                    OnPropertyChanged(
                        nameof(CanDownload));
                    OnPropertyChanged(
                        nameof(CanResume));
                }
            }
        }

        public long InitialPartialBytes
        {
            get => _initialPartialBytes;
            set => SetField(
                ref _initialPartialBytes,
                value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!CanDownload && !CanInstall)
                {
                    value = false;
                }

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
                {
                    OnPropertyChanged(
                        nameof(ProgressText));
                }
            }
        }

        public string ProgressText =>
            $"{Percentage:0}%";

        public string? FilePath
        {
            get => _filePath;
            set
            {
                if (SetField(ref _filePath, value))
                {
                    NotifyAvailabilityChanged();
                }
            }
        }

        public InstallerManifest? Manifest
        {
            get => _manifest;
            set
            {
                if (SetField(ref _manifest, value))
                {
                    NotifyAvailabilityChanged();
                }
            }
        }

        public bool IsInstalled
        {
            get => _isInstalled;
            set
            {
                if (SetField(ref _isInstalled, value))
                {
                    NotifyAvailabilityChanged();
                }
            }
        }

        public void NotifyAvailabilityChanged()
        {
            OnPropertyChanged(nameof(CanDownload));
            OnPropertyChanged(nameof(CanInstall));
            OnPropertyChanged(nameof(CanResume));
            OnPropertyChanged(nameof(IsSelected));
        }

        public event PropertyChangedEventHandler?
            PropertyChanged;

        private bool SetField<T>(
            ref T field,
            T value,
            [CallerMemberName]
            string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(
                    field,
                    value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(
            [CallerMemberName]
            string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    propertyName));
        }
    }
}
