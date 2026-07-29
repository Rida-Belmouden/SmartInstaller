using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SmartInstaller.Agent.Models;
using SmartInstaller.Agent.Services;

namespace SmartInstaller.Agent;

public partial class MainWindow : Window
{
    private readonly IInstalledSoftwareScanner _scanner;
    private readonly ObservableCollection<InstalledApplication> _applications = [];
    private readonly ICollectionView _applicationsView;
    private CancellationTokenSource? _scanCancellationTokenSource;

    public MainWindow()
    {
        InitializeComponent();

        var normalizer = new ApplicationNameNormalizer();
        _scanner = new InstalledSoftwareScanner(normalizer);

        var architectureDetector = new SystemArchitectureDetector();
        ArchitectureText.Text = architectureDetector.Detect();

        ApplicationsGrid.ItemsSource = _applications;
        _applicationsView = CollectionViewSource.GetDefaultView(_applications);
        _applicationsView.Filter = FilterApplication;
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        _scanCancellationTokenSource?.Dispose();
        _scanCancellationTokenSource = new CancellationTokenSource();

        SetScanningState(isScanning: true);
        StatusText.Text = "Scanning the Windows registry...";

        try
        {
            var applications = await _scanner.ScanAsync(
                _scanCancellationTokenSource.Token);

            _applications.Clear();

            foreach (var application in applications)
            {
                _applications.Add(application);
            }

            RefreshView();
            StatusText.Text = $"Scan completed. Found {applications.Count} applications.";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Scan canceled.";
        }
        catch (Exception exception)
        {
            StatusText.Text = "The scan failed.";

            MessageBox.Show(
                this,
                exception.Message,
                "SmartInstaller Agent",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            SetScanningState(isScanning: false);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _scanCancellationTokenSource?.Cancel();
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
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

        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return Contains(application.Name, search) ||
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

    private void SetScanningState(bool isScanning)
    {
        ScanButton.IsEnabled = !isScanning;
        CancelButton.IsEnabled = isScanning;
        SearchTextBox.IsEnabled = !isScanning;
    }

    private void ApplicationsGrid_Sorting(
        object sender,
        DataGridSortingEventArgs e)
    {
        StatusText.Text = $"Sorted by {e.Column.Header}.";
    }

    protected override void OnClosed(EventArgs e)
    {
        _scanCancellationTokenSource?.Cancel();
        _scanCancellationTokenSource?.Dispose();
        base.OnClosed(e);
    }
}
