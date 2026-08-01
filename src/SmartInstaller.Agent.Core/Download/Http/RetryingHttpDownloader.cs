using SmartInstaller.Agent.Core.Download.Retry;
namespace SmartInstaller.Agent.Core.Download.Http;

public sealed class RetryingHttpDownloader(
    IHttpDownloadAttemptExecutor attemptExecutor,
    IRetryPolicy retryPolicy,
    IRetryDelay retryDelay) : IHttpDownloader
{
    public async Task<HttpDownloadResult> DownloadAsync(HttpDownloadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var completedAttempt = 0;
        while (true)
        {
            completedAttempt++;
            DeletePartial(request.DestinationPath);
            var result = await attemptExecutor.ExecuteAsync(request, cancellationToken);
            var decision = retryPolicy.Evaluate(completedAttempt, result);
            if (!decision.ShouldRetry) return result;
            DeletePartial(request.DestinationPath);
            try
            {
                await retryDelay.WaitAsync(decision.Delay, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                DeletePartial(request.DestinationPath);
                return HttpDownloadResult.CancelledResult();
            }
        }
    }

    private static void DeletePartial(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }
}
