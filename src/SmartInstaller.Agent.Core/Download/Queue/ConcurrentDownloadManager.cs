using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;
using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Agent.Core.Download.Queue;

public sealed class ConcurrentDownloadManager(
    IUpdateDownloadService updateDownloadService,
    IOptions<ConcurrentDownloadOptions> options)
    : IConcurrentDownloadManager
{
    private readonly object _sync = new();
    private Dictionary<UpdateKey, ItemCancellation>? _activeItems;

    private readonly int _maximumParallelDownloads =
        ValidateMaximumParallelDownloads(
            options.Value.MaximumParallelDownloads);

    public async Task<ConcurrentDownloadResult> DownloadAsync(
        IReadOnlyCollection<UpdateCheckItem> updates,
        IProgress<DownloadQueueItemProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updates);

        var uniqueUpdates = RemoveDuplicates(updates);

        if (uniqueUpdates.Count == 0)
        {
            return new ConcurrentDownloadResult([]);
        }

        var activeItems = uniqueUpdates.ToDictionary(
            CreateKey,
            _ => new ItemCancellation(cancellationToken));

        lock (_sync)
        {
            if (_activeItems is not null)
            {
                throw new InvalidOperationException(
                    "A concurrent download operation is already running.");
            }

            _activeItems = activeItems;
        }

        try
        {
            using var semaphore = new SemaphoreSlim(
                _maximumParallelDownloads,
                _maximumParallelDownloads);

            var tasks = uniqueUpdates
                .Select((update, index) => DownloadItemAsync(
                    index,
                    update,
                    semaphore,
                    progress,
                    activeItems[CreateKey(update)]))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            return new ConcurrentDownloadResult(
                results.OrderBy(item => item.Index).ToArray());
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(_activeItems, activeItems))
                {
                    _activeItems = null;
                }
            }

            foreach (var item in activeItems.Values)
            {
                item.Dispose();
            }
        }
    }

    public bool PauseItem(UpdateCheckItem update) =>
        CancelItem(update, DownloadQueueCancellationReason.PauseItem);

    public bool ResumeItem(UpdateCheckItem update)
    {
        ArgumentNullException.ThrowIfNull(update);

        lock (_sync)
        {
            return _activeItems?.TryGetValue(
                       CreateKey(update),
                       out var item) == true &&
                   item.Resume();
        }
    }

    public bool CancelItem(UpdateCheckItem update) =>
        CancelItem(update, DownloadQueueCancellationReason.CancelItem);

    private async Task<DownloadQueueItemResult> DownloadItemAsync(
        int index,
        UpdateCheckItem update,
        SemaphoreSlim semaphore,
        IProgress<DownloadQueueItemProgress>? queueProgress,
        ItemCancellation itemCancellation)
    {
        Report(
            queueProgress,
            index,
            update,
            DownloadQueueStatus.Queued,
            null,
            null);

        while (true)
        {
            var cancellationToken = itemCancellation.Token;
            var enteredSemaphore = false;
            UpdateDownloadResult? cancelledResult = null;
            string? cancellationMessage = null;

            try
            {
                await semaphore.WaitAsync(cancellationToken);
                enteredSemaphore = true;

                Report(
                    queueProgress,
                    index,
                    update,
                    DownloadQueueStatus.Starting,
                    null,
                    null);

                var itemProgress =
                    new InlineProgress<DownloadProgress>(value =>
                        Report(
                            queueProgress,
                            index,
                            update,
                            DownloadQueueStatus.Downloading,
                            value,
                            null));

                var result =
                    await updateDownloadService.DownloadAsync(
                        update,
                        itemProgress,
                        cancellationToken);

                if (result.DownloadResult.Status ==
                    DownloadStatus.Cancelled)
                {
                    cancelledResult = result;
                    cancellationMessage =
                        result.DownloadResult.ErrorMessage;
                }
                else if (!result.DownloadResult.IsSuccess)
                {
                    var errorMessage =
                        result.DownloadResult.ErrorMessage ??
                        "The download failed.";

                    Report(
                        queueProgress,
                        index,
                        update,
                        DownloadQueueStatus.Failed,
                        null,
                        errorMessage);

                    return new DownloadQueueItemResult(
                        index,
                        update,
                        DownloadQueueStatus.Failed,
                        result,
                        errorMessage);
                }
                else
                {
                    Report(
                        queueProgress,
                        index,
                        update,
                        DownloadQueueStatus.Completed,
                        null,
                        null);

                    return new DownloadQueueItemResult(
                        index,
                        update,
                        DownloadQueueStatus.Completed,
                        result,
                        null);
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                cancellationMessage =
                    "The download was cancelled.";
            }
            catch (Exception exception)
            {
                Report(
                    queueProgress,
                    index,
                    update,
                    DownloadQueueStatus.Failed,
                    null,
                    exception.Message);

                return new DownloadQueueItemResult(
                    index,
                    update,
                    DownloadQueueStatus.Failed,
                    null,
                    exception.Message);
            }
            finally
            {
                if (enteredSemaphore)
                {
                    semaphore.Release();
                }
            }

            var cancellationReason =
                itemCancellation.GetReason();

            if (cancellationReason ==
                DownloadQueueCancellationReason.PauseItem)
            {
                Report(
                    queueProgress,
                    index,
                    update,
                    DownloadQueueStatus.Paused,
                    null,
                    "Paused");

                cancellationReason =
                    await itemCancellation
                        .WaitForResumeOrCancellationAsync();

                if (cancellationReason ==
                    DownloadQueueCancellationReason.None)
                {
                    Report(
                        queueProgress,
                        index,
                        update,
                        DownloadQueueStatus.Queued,
                        null,
                        "Queued to resume");

                    continue;
                }
            }

            Report(
                queueProgress,
                index,
                update,
                DownloadQueueStatus.Cancelled,
                null,
                cancellationMessage);

            return new DownloadQueueItemResult(
                index,
                update,
                DownloadQueueStatus.Cancelled,
                cancelledResult,
                cancellationMessage)
            {
                CancellationReason = cancellationReason
            };
        }
    }

    private static IReadOnlyList<UpdateCheckItem> RemoveDuplicates(
        IReadOnlyCollection<UpdateCheckItem> updates)
    {
        var result = new List<UpdateCheckItem>(updates.Count);
        var keys = new HashSet<(Guid ApplicationId, Guid? InstallerProfileId)>();

        foreach (var update in updates)
        {
            var key = (
                update.ApplicationId,
                update.InstallerProfileId);

            if (keys.Add(key))
            {
                result.Add(update);
            }
        }

        return result;
    }

    private static int ValidateMaximumParallelDownloads(int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ConcurrentDownloadOptions.MaximumParallelDownloads),
                value,
                "Maximum parallel downloads must be greater than zero.");
        }

        return value;
    }

    private bool CancelItem(
        UpdateCheckItem update,
        DownloadQueueCancellationReason reason)
    {
        ArgumentNullException.ThrowIfNull(update);

        lock (_sync)
        {
            return _activeItems?.TryGetValue(
                       CreateKey(update),
                       out var item) == true &&
                   item.Cancel(reason);
        }
    }

    private static UpdateKey CreateKey(UpdateCheckItem update) =>
        new(
            update.ApplicationId,
            update.InstallerProfileId);

    private static void Report(
        IProgress<DownloadQueueItemProgress>? progress,
        int index,
        UpdateCheckItem update,
        DownloadQueueStatus status,
        DownloadProgress? itemProgress,
        string? message)
    {
        progress?.Report(
            new DownloadQueueItemProgress(
                index,
                update,
                status,
                itemProgress,
                message));
    }

    private sealed class InlineProgress<T>(Action<T> callback)
        : IProgress<T>
    {
        public void Report(T value) => callback(value);
    }

    private readonly record struct UpdateKey(
        Guid ApplicationId,
        Guid? InstallerProfileId);

    private sealed class ItemCancellation : IDisposable
    {
        private readonly object _sync = new();
        private readonly CancellationToken _sessionToken;
        private CancellationTokenSource _source;
        private readonly CancellationTokenRegistration _sessionRegistration;
        private TaskCompletionSource<DownloadQueueCancellationReason>?
            _pauseCompletion;
        private DownloadQueueCancellationReason _reason;

        public ItemCancellation(CancellationToken sessionToken)
        {
            _sessionToken = sessionToken;
            _source =
                CancellationTokenSource.CreateLinkedTokenSource(
                    sessionToken);

            _sessionRegistration = sessionToken.Register(() =>
            {
                lock (_sync)
                {
                    _reason =
                        DownloadQueueCancellationReason.CancelAll;

                    _pauseCompletion?.TrySetResult(
                        DownloadQueueCancellationReason.CancelAll);
                }
            });
        }

        public CancellationToken Token
        {
            get
            {
                lock (_sync)
                {
                    return _source.Token;
                }
            }
        }

        public bool Cancel(DownloadQueueCancellationReason reason)
        {
            lock (_sync)
            {
                if (_source.IsCancellationRequested)
                {
                    if (_reason ==
                            DownloadQueueCancellationReason.PauseItem &&
                        reason ==
                            DownloadQueueCancellationReason.CancelItem)
                    {
                        _reason = reason;
                        _pauseCompletion?.TrySetResult(reason);
                        return true;
                    }

                    return false;
                }

                _reason = reason;

                if (reason ==
                    DownloadQueueCancellationReason.PauseItem)
                {
                    _pauseCompletion =
                        new TaskCompletionSource<
                            DownloadQueueCancellationReason>(
                            TaskCreationOptions
                                .RunContinuationsAsynchronously);
                }

                _source.Cancel();
                return true;
            }
        }

        public bool Resume()
        {
            lock (_sync)
            {
                if (_reason !=
                        DownloadQueueCancellationReason.PauseItem ||
                    _pauseCompletion is null)
                {
                    return false;
                }

                var pauseCompletion = _pauseCompletion;
                var oldSource = _source;

                _source =
                    CancellationTokenSource.CreateLinkedTokenSource(
                        _sessionToken);
                _reason =
                    DownloadQueueCancellationReason.None;
                _pauseCompletion = null;

                oldSource.Dispose();
                pauseCompletion.TrySetResult(
                    DownloadQueueCancellationReason.None);

                return true;
            }
        }

        public Task<DownloadQueueCancellationReason>
            WaitForResumeOrCancellationAsync()
        {
            lock (_sync)
            {
                return _pauseCompletion?.Task ??
                    Task.FromResult(_reason);
            }
        }

        public DownloadQueueCancellationReason GetReason()
        {
            lock (_sync)
            {
                return _reason == DownloadQueueCancellationReason.None &&
                       _source.IsCancellationRequested
                    ? DownloadQueueCancellationReason.CancelAll
                    : _reason;
            }
        }

        public void Dispose()
        {
            _sessionRegistration.Dispose();
            _source.Dispose();
        }
    }
}
