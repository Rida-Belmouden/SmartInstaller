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

        using var semaphore = new SemaphoreSlim(
            _maximumParallelDownloads,
            _maximumParallelDownloads);

        var tasks = uniqueUpdates
            .Select((update, index) => DownloadItemAsync(
                index,
                update,
                semaphore,
                progress,
                cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        return new ConcurrentDownloadResult(
            results.OrderBy(item => item.Index).ToArray());
    }

    private async Task<DownloadQueueItemResult> DownloadItemAsync(
        int index,
        UpdateCheckItem update,
        SemaphoreSlim semaphore,
        IProgress<DownloadQueueItemProgress>? queueProgress,
        CancellationToken cancellationToken)
    {
        Report(
            queueProgress,
            index,
            update,
            DownloadQueueStatus.Queued,
            null,
            null);

        var enteredSemaphore = false;

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

            var itemProgress = new InlineProgress<DownloadProgress>(value =>
                Report(
                    queueProgress,
                    index,
                    update,
                    DownloadQueueStatus.Downloading,
                    value,
                    null));

            var result = await updateDownloadService.DownloadAsync(
                update,
                itemProgress,
                cancellationToken);

            if (result.DownloadResult.Status ==
                DownloadStatus.Cancelled)
            {
                Report(
                    queueProgress,
                    index,
                    update,
                    DownloadQueueStatus.Cancelled,
                    null,
                    result.DownloadResult.ErrorMessage);

                return new DownloadQueueItemResult(
                    index,
                    update,
                    DownloadQueueStatus.Cancelled,
                    result,
                    result.DownloadResult.ErrorMessage);
            }

            if (!result.DownloadResult.IsSuccess)
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
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            Report(
                queueProgress,
                index,
                update,
                DownloadQueueStatus.Cancelled,
                null,
                "The download was cancelled.");

            return new DownloadQueueItemResult(
                index,
                update,
                DownloadQueueStatus.Cancelled,
                null,
                "The download was cancelled.");
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
}
