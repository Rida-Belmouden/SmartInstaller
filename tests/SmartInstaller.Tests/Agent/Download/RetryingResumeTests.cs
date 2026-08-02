using System.Net;
using SmartInstaller.Agent.Core.Download.Http;
using SmartInstaller.Agent.Core.Download.Resume;
using SmartInstaller.Agent.Core.Download.Retry;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class RetryingResumeTests
{
    [Fact]
    public async Task DownloadAsync_RetryUsesCurrentPartialLength()
    {
        var path = CreateTemporaryPath();

        try
        {
            var executor =
                new GrowingAttemptExecutor(path);

            var downloader =
                new RetryingHttpDownloader(
                    executor,
                    new TwoAttemptPolicy(),
                    new NoDelay(),
                    new ResumePolicy());

            var result =
                await downloader.DownloadAsync(
                    new HttpDownloadRequest(
                        new Uri("https://example.test/file.bin"),
                        path,
                        10,
                        null));

            Assert.True(result.Success);
            Assert.Equal(
                [0L, 4L],
                executor.Offsets);
        }
        finally
        {
            DeleteDirectory(path);
        }
    }

    [Fact]
    public async Task DownloadAsync_CancellationKeepsPartialFile()
    {
        var path = CreateTemporaryPath();

        try
        {
            await File.WriteAllBytesAsync(
                path,
                new byte[4]);

            var downloader =
                new RetryingHttpDownloader(
                    new CancelledExecutor(),
                    new StopPolicy(),
                    new NoDelay(),
                    new ResumePolicy());

            var result =
                await downloader.DownloadAsync(
                    new HttpDownloadRequest(
                        new Uri("https://example.test/file.bin"),
                        path,
                        10,
                        null));

            Assert.True(result.Cancelled);
            Assert.True(File.Exists(path));
            Assert.Equal(
                4,
                new FileInfo(path).Length);
        }
        finally
        {
            DeleteDirectory(path);
        }
    }

    private static string CreateTemporaryPath()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "SmartInstaller.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);

        return Path.Combine(
            directory,
            "file.bin.download");
    }

    private static void DeleteDirectory(
        string filePath)
    {
        var directory =
            Path.GetDirectoryName(filePath);

        if (directory is not null &&
            Directory.Exists(directory))
        {
            Directory.Delete(
                directory,
                true);
        }
    }

    private sealed class GrowingAttemptExecutor(
        string path)
        : IHttpDownloadAttemptExecutor
    {
        public List<long> Offsets { get; } = [];

        public async Task<HttpDownloadResult> ExecuteAsync(
            HttpDownloadRequest request,
            CancellationToken cancellationToken = default)
        {
            Offsets.Add(
                request.ResumeOffsetBytes);

            if (Offsets.Count == 1)
            {
                await File.WriteAllBytesAsync(
                    path,
                    new byte[4],
                    cancellationToken);

                return HttpDownloadResult.Failed(
                    "temporary",
                    HttpStatusCode.ServiceUnavailable);
            }

            await using var stream = new FileStream(
                path,
                FileMode.Append,
                FileAccess.Write);

            await stream.WriteAsync(
                new byte[6],
                cancellationToken);

            return HttpDownloadResult.Completed(
                10,
                wasResumed: true);
        }
    }

    private sealed class CancelledExecutor
        : IHttpDownloadAttemptExecutor
    {
        public Task<HttpDownloadResult> ExecuteAsync(
            HttpDownloadRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                HttpDownloadResult.CancelledResult());
        }
    }

    private sealed class TwoAttemptPolicy
        : IRetryPolicy
    {
        private int _calls;

        public RetryDecision Evaluate(
            int completedAttempt,
            HttpDownloadResult result)
        {
            _calls++;

            return _calls == 1
                ? RetryDecision.Retry(
                    TimeSpan.Zero,
                    RetryReason.ServerError)
                : RetryDecision.Stop(
                    RetryReason.None);
        }
    }

    private sealed class StopPolicy
        : IRetryPolicy
    {
        public RetryDecision Evaluate(
            int completedAttempt,
            HttpDownloadResult result)
        {
            return RetryDecision.Stop(
                RetryReason.None);
        }
    }

    private sealed class NoDelay
        : IRetryDelay
    {
        public Task WaitAsync(
            TimeSpan delay,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
