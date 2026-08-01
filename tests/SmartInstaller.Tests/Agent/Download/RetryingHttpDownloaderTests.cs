using System.Net;
using SmartInstaller.Agent.Core.Download.Http;
using SmartInstaller.Agent.Core.Download.Retry;
namespace SmartInstaller.Tests.Agent.Download;

public sealed class RetryingHttpDownloaderTests
{
    [Fact]
    public async Task DownloadAsync_SucceedsAfterTransientFailure()
    {
        var executor = new SequenceExecutor(
            HttpDownloadResult.Failed("busy", HttpStatusCode.ServiceUnavailable),
            HttpDownloadResult.Completed(100));
        var delay = new RecordingDelay();
        var downloader = new RetryingHttpDownloader(executor, new FixedPolicy(3), delay);
        var result = await downloader.DownloadAsync(CreateRequest());
        Assert.True(result.Success);
        Assert.Equal(2, executor.AttemptCount);
        Assert.Single(delay.Delays);
    }

    [Fact]
    public async Task DownloadAsync_StopsAfterMaximumAttempts()
    {
        var executor = new SequenceExecutor(HttpDownloadResult.Failed("busy", HttpStatusCode.ServiceUnavailable));
        var delay = new RecordingDelay();
        var result = await new RetryingHttpDownloader(executor, new FixedPolicy(3), delay).DownloadAsync(CreateRequest());
        Assert.False(result.Success);
        Assert.Equal(3, executor.AttemptCount);
        Assert.Equal(2, delay.Delays.Count);
    }

    [Fact]
    public async Task DownloadAsync_DoesNotRetryPermanentFailure()
    {
        var executor = new SequenceExecutor(HttpDownloadResult.Failed("not found", HttpStatusCode.NotFound));
        var result = await new RetryingHttpDownloader(executor, new FixedPolicy(4), new RecordingDelay()).DownloadAsync(CreateRequest());
        Assert.False(result.Success);
        Assert.Equal(1, executor.AttemptCount);
    }

    [Fact]
    public async Task DownloadAsync_CancellationDuringDelay_ReturnsCancelled()
    {
        var executor = new SequenceExecutor(HttpDownloadResult.Failed("busy", HttpStatusCode.ServiceUnavailable));
        var downloader = new RetryingHttpDownloader(executor, new FixedPolicy(4), new CancellingDelay());
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
        var result = await downloader.DownloadAsync(CreateRequest(), cancellation.Token);
        Assert.True(result.Cancelled);
    }

    private static HttpDownloadRequest CreateRequest()
    {
        var directory = Path.Combine(Path.GetTempPath(), "SmartInstaller.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return new HttpDownloadRequest(new Uri("https://example.test/setup.exe"), Path.Combine(directory, "setup.exe.download"), null, null);
    }

    private sealed class SequenceExecutor(params HttpDownloadResult[] results) : IHttpDownloadAttemptExecutor
    {
        private readonly Queue<HttpDownloadResult> _results = new(results);
        public int AttemptCount { get; private set; }
        public Task<HttpDownloadResult> ExecuteAsync(HttpDownloadRequest request, CancellationToken cancellationToken = default)
        {
            AttemptCount++;
            var result = _results.Count > 1 ? _results.Dequeue() : _results.Peek();
            return Task.FromResult(result);
        }
    }

    private sealed class FixedPolicy(int maxAttempts) : IRetryPolicy
    {
        public RetryDecision Evaluate(int completedAttempt, HttpDownloadResult result)
        {
            if (result.Success || result.Cancelled) return RetryDecision.Stop(RetryReason.None);
            if (completedAttempt >= maxAttempts) return RetryDecision.Stop(RetryReason.MaxAttemptsReached);
            if (result.StatusCode == HttpStatusCode.NotFound) return RetryDecision.Stop(RetryReason.PermanentFailure);
            return RetryDecision.Retry(TimeSpan.FromMilliseconds(1), RetryReason.ServerError);
        }
    }

    private sealed class RecordingDelay : IRetryDelay
    {
        public List<TimeSpan> Delays { get; } = [];
        public Task WaitAsync(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            Delays.Add(delay);
            return Task.CompletedTask;
        }
    }

    private sealed class CancellingDelay : IRetryDelay
    {
        public Task WaitAsync(TimeSpan delay, CancellationToken cancellationToken = default) =>
            Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
    }
}
