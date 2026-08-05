using SmartInstaller.Agent.Core.Download.Cache;
using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Queue;
using SmartInstaller.Agent.Core.Download.Resume;
using SmartInstaller.Agent.Core.Download.Session;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class DownloadSessionControllerTests
{
    [Fact]
    public async Task StartAsync_PublishesQueueSpeedAndEtaTelemetry()
    {
        var updates = CreateUpdates(2);
        var events = new List<DownloadSessionEvent>();
        var controller = new DownloadSessionController(
            new TelemetryDownloadManager(),
            new NullDownloadStateService(),
            new NullFileCacheService());

        controller.SessionEvent += (_, sessionEvent) =>
            events.Add(sessionEvent);

        var result = await controller.StartAsync(updates);

        Assert.Equal(2, result.CompletedCount);

        Assert.Contains(events, sessionEvent =>
            sessionEvent.Item?.Update.ApplicationId ==
                updates[1].ApplicationId &&
            sessionEvent.Item.QueuePosition == 1);

        var downloading = Assert.Single(
            events,
            sessionEvent =>
                sessionEvent.Item?.Update.ApplicationId ==
                    updates[0].ApplicationId &&
                sessionEvent.Item.BytesPerSecond == 25);

        Assert.Equal(
            TimeSpan.FromSeconds(3),
            downloading.Item?.RemainingTime);

        Assert.Contains(events, sessionEvent =>
            sessionEvent.Statistics is
            {
                Active: 1,
                Queued: 1,
                BytesPerSecond: 25
            });

        Assert.Contains(events, sessionEvent =>
            sessionEvent.Statistics is
            {
                Completed: 2,
                Active: 0,
                Queued: 0
            });
    }

    private static IReadOnlyList<UpdateCheckItem> CreateUpdates(
        int count) =>
        Enumerable.Range(1, count)
            .Select(index => new UpdateCheckItem(
                Guid.NewGuid(),
                $"Application {index}",
                "1.0",
                "2.0",
                true,
                Guid.NewGuid()))
            .ToArray();

    private sealed class TelemetryDownloadManager
        : IConcurrentDownloadManager
    {
        public Task<ConcurrentDownloadResult> DownloadAsync(
            IReadOnlyCollection<UpdateCheckItem> updates,
            IProgress<DownloadQueueItemProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var items = updates.ToArray();

            foreach (var item in items)
            {
                Report(
                    progress,
                    item,
                    DownloadQueueStatus.Queued);
            }

            Report(
                progress,
                items[0],
                DownloadQueueStatus.Starting);
            Report(
                progress,
                items[0],
                DownloadQueueStatus.Downloading,
                new DownloadProgress(
                    25,
                    100,
                    25,
                    25));
            Report(
                progress,
                items[0],
                DownloadQueueStatus.Completed);

            Report(
                progress,
                items[1],
                DownloadQueueStatus.Starting);
            Report(
                progress,
                items[1],
                DownloadQueueStatus.Downloading,
                new DownloadProgress(
                    50,
                    100,
                    50,
                    10));
            Report(
                progress,
                items[1],
                DownloadQueueStatus.Completed);

            return Task.FromResult(
                new ConcurrentDownloadResult(
                    items
                        .Select((item, index) =>
                            new DownloadQueueItemResult(
                                index,
                                item,
                                DownloadQueueStatus.Completed,
                                new UpdateDownloadResult(
                                    item,
                                    null,
                                    DownloadResult.Completed(
                                        $@"C:\Cache\{index}.exe",
                                        TimeSpan.Zero)),
                                null))
                        .ToArray()));
        }

        public bool PauseItem(UpdateCheckItem update) =>
            false;

        public bool ResumeItem(UpdateCheckItem update) =>
            false;

        public bool CancelItem(UpdateCheckItem update) =>
            false;

        private static void Report(
            IProgress<DownloadQueueItemProgress>? progress,
            UpdateCheckItem update,
            DownloadQueueStatus status,
            DownloadProgress? itemProgress = null) =>
            progress?.Report(
                new DownloadQueueItemProgress(
                    0,
                    update,
                    status,
                    itemProgress,
                    null));
    }

    private sealed class NullDownloadStateService
        : IUpdateDownloadStateService
    {
        public Task<UpdateDownloadState?> GetStateAsync(
            UpdateCheckItem update,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<UpdateDownloadState?>(null);
    }

    private sealed class NullFileCacheService
        : IFileCacheService
    {
        public string GetFinalPath(string fileName) =>
            string.Empty;

        public string GetTemporaryPath(string fileName) =>
            string.Empty;

        public bool IsReusable(
            string fileName,
            long? expectedFileSizeBytes) =>
            false;

        public void EnsureCacheDirectoryExists()
        {
        }

        public void DeleteTemporaryFile(string fileName)
        {
        }

        public void DeleteFinalFile(string fileName)
        {
        }

        public void PromoteTemporaryFile(
            string fileName,
            bool overwrite)
        {
        }

        public long GetTemporaryFileSize(string fileName) =>
            0;

        public ResumeMetadata GetResumeMetadata(
            string fileName) =>
            new(string.Empty, false, 0);
    }
}
