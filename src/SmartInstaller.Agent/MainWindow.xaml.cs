using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Agent;

public partial class MainWindow : Window
{
    private readonly IInstalledSoftwareScanner _scanner;
    private readonly IUpdateSynchronizationService _synchronizationService;
    private readonly ObservableCollection<InstalledApplication> _applications = [];
    private readonly ObservableCollection<UpdateRow> _updates = [];
    private readonly ICollectionView _applicationsView;
    private CancellationTokenSource? _operationCancellationTokenSource;

    public MainWindow(
        IInstalledSoftwareScanner scanner,
        IUpdateSynchronizationService synchronizationService,
        ISystemArchitectureDetector architectureDetector)
    {
        InitializeComponent();
        _scanner = scanner;
        _synchronizationService = synchronizationService;
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
            {
                _applications.Add(application);
            }

            _updates.Clear();
            UpdatesTab.Header = "Updates (0)";
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

            _updates.Clear();
            foreach (var item in result.Items)
            {
                _updates.Add(new UpdateRow(
                    item.ApplicationName,
                    item.InstalledVersion,
                    item.LatestVersion,
                    item.UpdateAvailable ? "Update available" : "Up to date"));
            }

            UpdatesTab.Header = $"Updates ({result.UpdateCount})";
            StatusText.Text = result.MatchedApplicationCount == 0
                ? "No installed applications matched the current SmartInstaller catalog."
                : $"Matched {result.MatchedApplicationCount} applications. " +
                  $"Found {result.UpdateCount} updates.";
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
        {
            return false;
        }

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

    private sealed record UpdateRow(
        string ApplicationName,
        string InstalledVersion,
        string LatestVersion,
        string Status);
}
