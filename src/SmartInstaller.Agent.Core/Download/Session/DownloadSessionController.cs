using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Cache;
using SmartInstaller.Agent.Core.Download.Queue;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Agent.Core.Download.Session;

public sealed class DownloadSessionController(
    IConcurrentDownloadManager downloadManager,
    IUpdateDownloadStateService downloadStateService,
    IFileCacheService fileCacheService)
    : IDownloadSessionController, IDisposable
{
    private readonly object _sync = new();
    private readonly object _telemetrySync = new();
    private CancellationTokenSource? _sessionCancellation;
    private IReadOnlyList<UpdateKey> _itemOrder = [];
    private Dictionary<UpdateKey, TelemetryEntry> _telemetry = [];

    public event EventHandler<DownloadSessionEvent>? SessionEvent;

    public bool IsRunning
    {
        get
        {
            lock (_sync)
            {
                return _sessionCancellation is not null;
            }
        }
    }

    public async Task<ConcurrentDownloadResult> StartAsync(
        IReadOnlyCollection<UpdateCheckItem> updates,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updates);

        if (updates.Count == 0)
        {
            throw new ArgumentException(
                "A download session requires at least one update.",
                nameof(updates));
        }

        CancellationTokenSource sessionCancellation;

        lock (_sync)
        {
            if (_sessionCancellation is not null)
            {
                throw new InvalidOperationException(
                    "A download session is already running.");
            }

            sessionCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            _sessionCancellation = sessionCancellation;
        }

        InitializeTelemetry(updates);

        Publish(new DownloadSessionEvent(
            DownloadSessionEventType.SessionStarted,
            Message: $"Downloading {updates.Count} update(s) concurrently..."));

        var initialPosition = 0;

        foreach (var update in updates)
        {
            initialPosition++;

            PublishItem(new DownloadSessionItemState(
                update,
                "Queued",
                0,
                true,
                false,
                0,
                CanPause: true,
                CanCancel: true,
                QueuePosition: initialPosition));
        }

        PublishStatistics(CreateStatistics());

        try
        {
            var progress =
                new InlineProgress<DownloadQueueItemProgress>(
                    PublishProgress);

            var result = await downloadManager.DownloadAsync(
                updates,
                progress,
                sessionCancellation.Token);

            foreach (var item in result.Items)
            {
                await PublishResultAsync(item);
            }

            var eventType = sessionCancellation.IsCancellationRequested
                ? DownloadSessionEventType.SessionCancelled
                : DownloadSessionEventType.SessionCompleted;

            Publish(new DownloadSessionEvent(
                eventType,
                Result: result,
                Message:
                    $"{result.CompletedCount} completed, " +
                    $"{result.FailedCount} failed, " +
                    $"{result.PausedCount} paused, " +
                    $"{result.CancelledCount} cancelled."));

            return result;
        }
        catch (OperationCanceledException)
            when (sessionCancellation.IsCancellationRequested)
        {
            await PublishInterruptedItemsAsync(
                updates,
                "Canceled");

            Publish(new DownloadSessionEvent(
                DownloadSessionEventType.SessionCancelled,
                Message: "Concurrent download operation canceled."));

            throw;
        }
        catch (Exception exception)
        {
            await PublishInterruptedItemsAsync(
                updates,
                "Paused after interruption");

            Publish(new DownloadSessionEvent(
                DownloadSessionEventType.SessionFailed,
                Message: exception.Message));

            throw;
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(
                        _sessionCancellation,
                        sessionCancellation))
                {
                    _sessionCancellation = null;
                }
            }

            sessionCancellation.Dispose();
        }
    }

    public void CancelAll()
    {
        lock (_sync)
        {
            _sessionCancellation?.Cancel();
        }
    }

    public bool PauseItem(UpdateCheckItem update) =>
        downloadManager.PauseItem(update);

    public bool ResumeItem(UpdateCheckItem update) =>
        downloadManager.ResumeItem(update);

    public bool CancelItem(UpdateCheckItem update) =>
        downloadManager.CancelItem(update);

    public async Task<bool> DiscardPartialDownloadAsync(
        UpdateCheckItem update,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);

        var state = await downloadStateService.GetStateAsync(
            update,
            cancellationToken);

        if (state?.HasPartialFile != true)
        {
            return false;
        }

        fileCacheService.DeleteTemporaryFile(
            state.FileName);

        PublishItem(new DownloadSessionItemState(
            update,
            "Canceled",
            0,
            false,
            false,
            0,
            state.Manifest));

        return true;
    }

    public void Dispose()
    {
        CancelAll();

        lock (_sync)
        {
            _sessionCancellation?.Dispose();
            _sessionCancellation = null;
        }
    }

    private void PublishProgress(DownloadQueueItemProgress item)
    {
        var telemetry = TrackProgress(item);
        var status = item.Status switch
        {
            DownloadQueueStatus.Queued => "Queued",
            DownloadQueueStatus.Starting => "Starting",
            DownloadQueueStatus.Downloading => "Downloading",
            DownloadQueueStatus.Paused => "Pausing",
            DownloadQueueStatus.Completed => "Verifying",
            DownloadQueueStatus.Failed =>
                item.Message ?? "Download failed",
            DownloadQueueStatus.Cancelled => "Pausing",
            _ => item.Status.ToString()
        };

        PublishItem(new DownloadSessionItemState(
            item.Update,
            status,
            item.Progress?.Percentage ?? 0,
            item.Status is not
                DownloadQueueStatus.Completed and not
                DownloadQueueStatus.Failed and not
                DownloadQueueStatus.Cancelled and not
                DownloadQueueStatus.Paused,
            false,
            0,
            CanPause: item.Status is
                DownloadQueueStatus.Queued or
                DownloadQueueStatus.Starting or
                DownloadQueueStatus.Downloading,
            CanResume: item.Status is
                DownloadQueueStatus.Paused,
            CanCancel: item.Status is
                DownloadQueueStatus.Queued or
                DownloadQueueStatus.Starting or
                DownloadQueueStatus.Downloading or
                DownloadQueueStatus.Paused,
            QueuePosition: telemetry.QueuePosition,
            BytesPerSecond:
                item.Progress?.BytesPerSecond ?? 0,
            RemainingTime:
                CalculateRemainingTime(item.Progress)));

        PublishQueuedPositions(
            telemetry.QueuedPositions,
            CreateKey(item.Update));
        PublishStatistics(telemetry.Statistics);
    }

    private async Task PublishResultAsync(
        DownloadQueueItemResult item)
    {
        var state = await downloadStateService.GetStateAsync(
            item.Update,
            CancellationToken.None);

        if (item.CancellationReason ==
                DownloadQueueCancellationReason.CancelItem &&
            state is not null)
        {
            fileCacheService.DeleteTemporaryFile(
                state.FileName);

            state = state with
            {
                PartialBytes = 0
            };
        }

        var downloadResult = item.DownloadResult;
        var percentage = state?.FinalFileExists == true
            ? 100
            : state?.Percentage ?? 0;

        var status = item.Status switch
        {
            DownloadQueueStatus.Completed
                when downloadResult?.DownloadResult.Status ==
                     DownloadStatus.Cached =>
                "Ready from cache",
            DownloadQueueStatus.Completed =>
                "Downloaded and verified",
            DownloadQueueStatus.Cancelled
                when item.CancellationReason ==
                     DownloadQueueCancellationReason.CancelItem =>
                "Canceled",
            DownloadQueueStatus.Cancelled
                when state?.HasPartialFile == true =>
                "Paused",
            DownloadQueueStatus.Cancelled =>
                "Canceled",
            DownloadQueueStatus.Paused
                when state?.HasPartialFile == true =>
                "Paused",
            DownloadQueueStatus.Paused =>
                "Paused",
            DownloadQueueStatus.Failed
                when state?.HasPartialFile == true &&
                     !string.IsNullOrWhiteSpace(item.ErrorMessage) =>
                $"Paused - {item.ErrorMessage}",
            DownloadQueueStatus.Failed
                when state?.HasPartialFile == true =>
                "Paused after failure",
            DownloadQueueStatus.Failed =>
                item.ErrorMessage ?? "Download failed",
            _ => item.Status.ToString()
        };

        var hasManageablePartial =
            state?.HasPartialFile == true &&
            item.CancellationReason !=
                DownloadQueueCancellationReason.CancelItem &&
            item.Status != DownloadQueueStatus.Completed;

        var telemetry = TrackResult(item);

        PublishItem(new DownloadSessionItemState(
            item.Update,
            status,
            percentage,
            false,
            state?.HasPartialFile == true,
            state?.PartialBytes ?? 0,
            downloadResult?.Manifest ?? state?.Manifest,
            downloadResult?.DownloadResult.FilePath ??
                (state?.FinalFileExists == true
                    ? state.FinalPath
                    : null),
            CanResume: hasManageablePartial,
            CanCancel: hasManageablePartial));

        PublishQueuedPositions(
            telemetry.QueuedPositions,
            CreateKey(item.Update));
        PublishStatistics(telemetry.Statistics);
    }

    private async Task PublishInterruptedItemsAsync(
        IReadOnlyCollection<UpdateCheckItem> updates,
        string fallbackStatus)
    {
        foreach (var update in updates)
        {
            var state = await downloadStateService.GetStateAsync(
                update,
                CancellationToken.None);

            PublishItem(new DownloadSessionItemState(
                update,
                state?.HasPartialFile == true
                    ? "Paused"
                    : fallbackStatus,
                state?.Percentage ?? 0,
                false,
                state?.HasPartialFile == true,
                state?.PartialBytes ?? 0,
                state?.Manifest,
                state?.FinalFileExists == true
                    ? state.FinalPath
                    : null,
                CanResume: state?.HasPartialFile == true,
                CanCancel: state?.HasPartialFile == true));
        }
    }

    private void PublishItem(DownloadSessionItemState item) =>
        Publish(new DownloadSessionEvent(
            DownloadSessionEventType.ItemUpdated,
            Item: item));

    private void InitializeTelemetry(
        IReadOnlyCollection<UpdateCheckItem> updates)
    {
        lock (_telemetrySync)
        {
            _itemOrder = updates
                .Select(CreateKey)
                .Distinct()
                .ToArray();

            _telemetry = updates
                .GroupBy(CreateKey)
                .ToDictionary(
                    group => group.Key,
                    group => new TelemetryEntry(
                        group.First(),
                        DownloadQueueStatus.Queued));
        }
    }

    private TelemetryUpdate TrackProgress(
        DownloadQueueItemProgress progress)
    {
        lock (_telemetrySync)
        {
            var key = CreateKey(progress.Update);

            if (!_telemetry.TryGetValue(
                    key,
                    out var entry))
            {
                entry = new TelemetryEntry(
                    progress.Update,
                    progress.Status);
                _telemetry[key] = entry;
            }

            entry.Status = progress.Status;
            entry.IsTerminal = false;
            entry.BytesPerSecond =
                progress.Progress?.BytesPerSecond ?? 0;

            return CreateTelemetryUpdate(key);
        }
    }

    private TelemetryUpdate TrackResult(
        DownloadQueueItemResult result)
    {
        lock (_telemetrySync)
        {
            var key = CreateKey(result.Update);

            if (!_telemetry.TryGetValue(
                    key,
                    out var entry))
            {
                entry = new TelemetryEntry(
                    result.Update,
                    result.Status);
                _telemetry[key] = entry;
            }

            entry.Status = result.Status;
            entry.IsTerminal = true;
            entry.BytesPerSecond = 0;

            return CreateTelemetryUpdate(key);
        }
    }

    private TelemetryUpdate CreateTelemetryUpdate(
        UpdateKey currentKey)
    {
        var queuedPositions = _itemOrder
            .Where(key =>
                _telemetry.TryGetValue(
                    key,
                    out var entry) &&
                !entry.IsTerminal &&
                entry.Status == DownloadQueueStatus.Queued)
            .Select((key, index) => new QueuePosition(
                key,
                _telemetry[key].Update,
                index + 1))
            .ToArray();

        var currentPosition = queuedPositions
            .FirstOrDefault(item =>
                item.Key == currentKey)
            ?.Position;

        return new TelemetryUpdate(
            currentPosition,
            queuedPositions,
            CreateStatisticsCore());
    }

    private DownloadSessionStatistics CreateStatistics()
    {
        lock (_telemetrySync)
        {
            return CreateStatisticsCore();
        }
    }

    private DownloadSessionStatistics CreateStatisticsCore()
    {
        var entries = _telemetry.Values.ToArray();

        return new DownloadSessionStatistics(
            entries.Length,
            entries.Count(item =>
                !item.IsTerminal &&
                item.Status == DownloadQueueStatus.Queued),
            entries.Count(item =>
                !item.IsTerminal &&
                item.Status is
                    DownloadQueueStatus.Starting or
                    DownloadQueueStatus.Downloading or
                    DownloadQueueStatus.Completed),
            entries.Count(item =>
                !item.IsTerminal &&
                item.Status == DownloadQueueStatus.Paused),
            entries.Count(item =>
                item.IsTerminal &&
                item.Status == DownloadQueueStatus.Completed),
            entries.Count(item =>
                item.IsTerminal &&
                item.Status == DownloadQueueStatus.Failed),
            entries.Count(item =>
                item.IsTerminal &&
                item.Status == DownloadQueueStatus.Cancelled),
            entries
                .Where(item =>
                    !item.IsTerminal &&
                    item.Status == DownloadQueueStatus.Downloading)
                .Sum(item => item.BytesPerSecond));
    }

    private void PublishQueuedPositions(
        IReadOnlyCollection<QueuePosition> positions,
        UpdateKey currentKey)
    {
        foreach (var position in positions)
        {
            if (position.Key == currentKey)
            {
                continue;
            }

            PublishItem(new DownloadSessionItemState(
                position.Update,
                $"Queued (#{position.Position})",
                0,
                true,
                false,
                0,
                CanPause: true,
                CanCancel: true,
                QueuePosition: position.Position));
        }
    }

    private void PublishStatistics(
        DownloadSessionStatistics statistics) =>
        Publish(new DownloadSessionEvent(
            DownloadSessionEventType.SnapshotUpdated,
            Statistics: statistics));

    private static TimeSpan? CalculateRemainingTime(
        DownloadProgress? progress)
    {
        if (progress?.TotalBytes is not > 0 ||
            progress.BytesPerSecond <= 0 ||
            progress.BytesReceived >= progress.TotalBytes.Value)
        {
            return null;
        }

        return TimeSpan.FromSeconds(
            (progress.TotalBytes.Value -
             progress.BytesReceived) /
            progress.BytesPerSecond);
    }

    private static UpdateKey CreateKey(
        UpdateCheckItem update) =>
        new(
            update.ApplicationId,
            update.InstallerProfileId);

    private void Publish(DownloadSessionEvent sessionEvent) =>
        SessionEvent?.Invoke(this, sessionEvent);

    private sealed class InlineProgress<T>(Action<T> callback)
        : IProgress<T>
    {
        public void Report(T value) => callback(value);
    }

    private readonly record struct UpdateKey(
        Guid ApplicationId,
        Guid? InstallerProfileId);

    private sealed class TelemetryEntry(
        UpdateCheckItem update,
        DownloadQueueStatus status)
    {
        public UpdateCheckItem Update { get; } = update;

        public DownloadQueueStatus Status { get; set; } =
            status;

        public bool IsTerminal { get; set; }

        public double BytesPerSecond { get; set; }
    }

    private sealed record QueuePosition(
        UpdateKey Key,
        UpdateCheckItem Update,
        int Position);

    private sealed record TelemetryUpdate(
        int? QueuePosition,
        IReadOnlyCollection<QueuePosition> QueuedPositions,
        DownloadSessionStatistics Statistics);
}
