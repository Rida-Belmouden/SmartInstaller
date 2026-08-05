using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;
using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Queue;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class ConcurrentDownloadManagerTests
{
    [Fact]
    public async Task DownloadAsync_RespectsMaximumParallelDownloads()
    {
        var service = new TrackingDownloadService(
            TimeSpan.FromMilliseconds(40));

        var manager = CreateManager(service, maximumParallel: 2);

        var result = await manager.DownloadAsync(
            CreateUpdates(6));

        Assert.Equal(6, result.CompletedCount);
        Assert.InRange(service.MaximumConcurrency, 1, 2);
        Assert.Equal(2, service.MaximumConcurrency);
    }

    [Fact]
    public async Task DownloadAsync_MaximumOne_RunsSequentially()
    {
        var service = new TrackingDownloadService(
            TimeSpan.FromMilliseconds(10));

        var result = await CreateManager(
                service,
                maximumParallel: 1)
            .DownloadAsync(CreateUpdates(4));

        Assert.Equal(4, result.CompletedCount);
        Assert.Equal(1, service.MaximumConcurrency);
    }

    [Fact]
    public async Task DownloadAsync_PreservesInputOrder()
    {
        var updates = CreateUpdates(5);
        var service = new TrackingDownloadService(
            TimeSpan.FromMilliseconds(5),
            reverseDelay: true);

        var result = await CreateManager(service, 3)
            .DownloadAsync(updates);

        Assert.Equal(
            updates.Select(item => item.ApplicationId),
            result.Items.Select(item => item.Update.ApplicationId));
    }

    [Fact]
    public async Task DownloadAsync_FailedItem_DoesNotStopOtherItems()
    {
        var updates = CreateUpdates(4);
        var failedId = updates[1].ApplicationId;

        var service = new TrackingDownloadService(
            TimeSpan.Zero,
            failedApplicationId: failedId);

        var result = await CreateManager(service, 2)
            .DownloadAsync(updates);

        Assert.Equal(3, result.CompletedCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Equal(0, result.CancelledCount);
    }

    [Fact]
    public async Task DownloadAsync_RemovesDuplicateApplications()
    {
        var update = CreateUpdates(1)[0];
        var service = new TrackingDownloadService(TimeSpan.Zero);

        var result = await CreateManager(service, 3)
            .DownloadAsync([update, update, update]);

        Assert.Single(result.Items);
        Assert.Equal(1, service.CallCount);
    }

    [Fact]
    public async Task DownloadAsync_Cancellation_CancelsQueuedAndRunningItems()
    {
        var service = new TrackingDownloadService(
            TimeSpan.FromSeconds(5));

        var manager = CreateManager(service, 2);

        using var cancellation =
            new CancellationTokenSource(
                TimeSpan.FromMilliseconds(50));

        var result = await manager.DownloadAsync(
            CreateUpdates(5),
            cancellationToken: cancellation.Token);

        Assert.Equal(5, result.TotalCount);
        Assert.True(result.CancelledCount >= 1);
    }

    [Fact]
    public async Task DownloadAsync_ReportsIndependentItemStates()
    {
        var reports = new List<DownloadQueueItemProgress>();
        var service = new TrackingDownloadService(TimeSpan.Zero);

        await CreateManager(service, 2).DownloadAsync(
            CreateUpdates(2),
            new InlineProgress<DownloadQueueItemProgress>(reports.Add));

        Assert.Contains(reports, item =>
            item.Status == DownloadQueueStatus.Queued);

        Assert.Contains(reports, item =>
            item.Status == DownloadQueueStatus.Starting);

        Assert.Contains(reports, item =>
            item.Status == DownloadQueueStatus.Downloading);

        Assert.Equal(
            2,
            reports.Count(item =>
                item.Status == DownloadQueueStatus.Completed));
    }

    [Fact]
    public async Task ResumeItem_ContinuesOnlyPausedItem()
    {
        var updates = CreateUpdates(3);
        var paused =
            new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
        var manager = CreateManager(
            new TrackingDownloadService(
                TimeSpan.FromMilliseconds(100)),
            maximumParallel: 2);

        var downloadTask = manager.DownloadAsync(
            updates,
            new InlineProgress<DownloadQueueItemProgress>(item =>
            {
                if (item.Update.ApplicationId ==
                        updates[0].ApplicationId &&
                    item.Status == DownloadQueueStatus.Paused)
                {
                    paused.TrySetResult();
                }
            }));

        Assert.True(manager.PauseItem(updates[0]));
        await paused.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(manager.ResumeItem(updates[0]));

        var result = await downloadTask;
        Assert.Equal(3, result.CompletedCount);
        Assert.Equal(0, result.CancelledCount);
    }

    [Fact]
    public async Task CancelItem_CancelsOnlyRequestedItem()
    {
        var updates = CreateUpdates(3);
        var manager = CreateManager(
            new TrackingDownloadService(
                TimeSpan.FromMilliseconds(100)),
            maximumParallel: 2);

        var downloadTask = manager.DownloadAsync(updates);

        Assert.True(manager.CancelItem(updates[1]));

        var result = await downloadTask;
        var cancelled = Assert.Single(result.Items, item =>
            item.Status == DownloadQueueStatus.Cancelled);

        Assert.Equal(
            updates[1].ApplicationId,
            cancelled.Update.ApplicationId);
        Assert.Equal(
            DownloadQueueCancellationReason.CancelItem,
            cancelled.CancellationReason);
        Assert.Equal(2, result.CompletedCount);
    }

    [Fact]
    public async Task CancelItem_WhenPaused_EndsOnlyPausedItem()
    {
        var updates = CreateUpdates(3);
        var paused =
            new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
        var manager = CreateManager(
            new TrackingDownloadService(
                TimeSpan.FromMilliseconds(100)),
            maximumParallel: 2);

        var downloadTask = manager.DownloadAsync(
            updates,
            new InlineProgress<DownloadQueueItemProgress>(item =>
            {
                if (item.Update.ApplicationId ==
                        updates[0].ApplicationId &&
                    item.Status == DownloadQueueStatus.Paused)
                {
                    paused.TrySetResult();
                }
            }));

        Assert.True(manager.PauseItem(updates[0]));
        await paused.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(manager.CancelItem(updates[0]));

        var result = await downloadTask;
        var cancelled = Assert.Single(result.Items, item =>
            item.Status == DownloadQueueStatus.Cancelled);

        Assert.Equal(
            DownloadQueueCancellationReason.CancelItem,
            cancelled.CancellationReason);
        Assert.Equal(2, result.CompletedCount);
    }

    [Fact]
    public async Task CancelAll_WhenItemPaused_ReleasesPausedItem()
    {
        var updates = CreateUpdates(2);
        var paused =
            new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
        var manager = CreateManager(
            new TrackingDownloadService(
                TimeSpan.FromSeconds(5)),
            maximumParallel: 2);
        using var cancellation = new CancellationTokenSource();

        var downloadTask = manager.DownloadAsync(
            updates,
            new InlineProgress<DownloadQueueItemProgress>(item =>
            {
                if (item.Update.ApplicationId ==
                        updates[0].ApplicationId &&
                    item.Status == DownloadQueueStatus.Paused)
                {
                    paused.TrySetResult();
                }
            }),
            cancellation.Token);

        Assert.True(manager.PauseItem(updates[0]));
        await paused.Task.WaitAsync(TimeSpan.FromSeconds(2));

        cancellation.Cancel();

        var result = await downloadTask.WaitAsync(
            TimeSpan.FromSeconds(2));

        Assert.Equal(2, result.CancelledCount);
        Assert.All(result.Items, item =>
            Assert.Equal(
                DownloadQueueCancellationReason.CancelAll,
                item.CancellationReason));
    }

    [Fact]
    public void ItemControl_WithoutActiveSession_ReturnsFalse()
    {
        var update = CreateUpdates(1)[0];
        var manager = CreateManager(
            new TrackingDownloadService(TimeSpan.Zero),
            maximumParallel: 1);

        Assert.False(manager.PauseItem(update));
        Assert.False(manager.ResumeItem(update));
        Assert.False(manager.CancelItem(update));
    }

    [Fact]
    public void Constructor_InvalidMaximumParallelDownloads_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateManager(
                new TrackingDownloadService(TimeSpan.Zero),
                0));
    }

    private static ConcurrentDownloadManager CreateManager(
        IUpdateDownloadService service,
        int maximumParallel)
    {
        return new ConcurrentDownloadManager(
            service,
            Options.Create(
                new ConcurrentDownloadOptions
                {
                    MaximumParallelDownloads = maximumParallel
                }));
    }

    private static IReadOnlyList<UpdateCheckItem> CreateUpdates(
        int count)
    {
        return Enumerable.Range(1, count)
            .Select(index => new UpdateCheckItem(
                Guid.NewGuid(),
                $"Application {index}",
                "1.0",
                "2.0",
                true,
                Guid.NewGuid()))
            .ToArray();
    }

    private sealed class TrackingDownloadService(
        TimeSpan delay,
        bool reverseDelay = false,
        Guid? failedApplicationId = null)
        : IUpdateDownloadService
    {
        private int _currentConcurrency;
        private int _maximumConcurrency;
        private int _callCount;

        public int MaximumConcurrency =>
            Volatile.Read(ref _maximumConcurrency);

        public int CallCount =>
            Volatile.Read(ref _callCount);

        public async Task<UpdateDownloadResult> DownloadAsync(
            UpdateCheckItem update,
            IProgress<DownloadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);

            var current = Interlocked.Increment(
                ref _currentConcurrency);

            UpdateMaximum(current);

            try
            {
                progress?.Report(
                    new DownloadProgress(
                        50,
                        100,
                        50,
                        1000));

                var effectiveDelay = reverseDelay
                    ? TimeSpan.FromMilliseconds(
                        Math.Max(1, 30 - CallCount * 4))
                    : delay;

                if (effectiveDelay > TimeSpan.Zero)
                {
                    await Task.Delay(
                        effectiveDelay,
                        cancellationToken);
                }

                if (update.ApplicationId == failedApplicationId)
                {
                    return new UpdateDownloadResult(
                        update,
                        null,
                        DownloadResult.Failed("failed"));
                }

                return new UpdateDownloadResult(
                    update,
                    null,
                    DownloadResult.Completed(
                        $@"C:\Cache\{update.ApplicationId}.exe",
                        TimeSpan.Zero));
            }
            finally
            {
                Interlocked.Decrement(
                    ref _currentConcurrency);
            }
        }

        private void UpdateMaximum(int candidate)
        {
            while (true)
            {
                var current = Volatile.Read(
                    ref _maximumConcurrency);

                if (candidate <= current)
                {
                    return;
                }

                if (Interlocked.CompareExchange(
                        ref _maximumConcurrency,
                        candidate,
                        current) == current)
                {
                    return;
                }
            }
        }
    }

    private sealed class InlineProgress<T>(Action<T> callback)
        : IProgress<T>
    {
        public void Report(T value) => callback(value);
    }
}
