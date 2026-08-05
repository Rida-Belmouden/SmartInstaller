using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Queue;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Agent.Core.Download.Session;

public sealed class DownloadSessionController(
    IConcurrentDownloadManager downloadManager,
    IUpdateDownloadStateService downloadStateService)
    : IDownloadSessionController, IDisposable
{
    private readonly object _sync = new();
    private CancellationTokenSource? _sessionCancellation;

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

        Publish(new DownloadSessionEvent(
            DownloadSessionEventType.SessionStarted,
            Message: $"Downloading {updates.Count} update(s) concurrently..."));

        foreach (var update in updates)
        {
            PublishItem(new DownloadSessionItemState(
                update,
                "Queued",
                0,
                true,
                false,
                0));
        }

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
        var status = item.Status switch
        {
            DownloadQueueStatus.Queued => "Queued",
            DownloadQueueStatus.Starting => "Starting",
            DownloadQueueStatus.Downloading => "Downloading",
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
                DownloadQueueStatus.Cancelled,
            false,
            0));
    }

    private async Task PublishResultAsync(
        DownloadQueueItemResult item)
    {
        var state = await downloadStateService.GetStateAsync(
            item.Update,
            CancellationToken.None);

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
                when state?.HasPartialFile == true =>
                "Paused",
            DownloadQueueStatus.Cancelled =>
                "Canceled",
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
                    : null)));
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
                    : null));
        }
    }

    private void PublishItem(DownloadSessionItemState item) =>
        Publish(new DownloadSessionEvent(
            DownloadSessionEventType.ItemUpdated,
            Item: item));

    private void Publish(DownloadSessionEvent sessionEvent) =>
        SessionEvent?.Invoke(this, sessionEvent);

    private sealed class InlineProgress<T>(Action<T> callback)
        : IProgress<T>
    {
        public void Report(T value) => callback(value);
    }
}
