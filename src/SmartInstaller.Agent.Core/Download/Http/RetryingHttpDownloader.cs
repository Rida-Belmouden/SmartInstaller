using SmartInstaller.Agent.Core.Download.Resume;
using SmartInstaller.Agent.Core.Download.Retry;

namespace SmartInstaller.Agent.Core.Download.Http;

public sealed class RetryingHttpDownloader
    : IHttpDownloader
{
    private readonly IHttpDownloadAttemptExecutor _attemptExecutor;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IRetryDelay _retryDelay;
    private readonly IResumePolicy _resumePolicy;

    public RetryingHttpDownloader(
        IHttpDownloadAttemptExecutor attemptExecutor,
        IRetryPolicy retryPolicy,
        IRetryDelay retryDelay,
        IResumePolicy? resumePolicy = null)
    {
        _attemptExecutor = attemptExecutor;
        _retryPolicy = retryPolicy;
        _retryDelay = retryDelay;
        _resumePolicy =
            resumePolicy ??
            new ResumePolicy();
    }

    public async Task<HttpDownloadResult> DownloadAsync(
        HttpDownloadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var completedAttempt = 0;

        while (true)
        {
            completedAttempt++;

            var attemptRequest =
                PrepareAttemptRequest(request);

            var result =
                await _attemptExecutor.ExecuteAsync(
                    attemptRequest,
                    cancellationToken);

            var decision =
                _retryPolicy.Evaluate(
                    completedAttempt,
                    result);

            if (!decision.ShouldRetry)
            {
                return result;
            }

            try
            {
                await _retryDelay.WaitAsync(
                    decision.Delay,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return HttpDownloadResult.CancelledResult();
            }
        }
    }

    private HttpDownloadRequest PrepareAttemptRequest(
        HttpDownloadRequest request)
    {
        var metadata =
            GetResumeMetadata(
                request.DestinationPath);

        var decision =
            _resumePolicy.Evaluate(
                metadata,
                request.ExpectedFileSizeBytes);

        if (decision.Mode ==
            ResumeMode.RestartDownload)
        {
            DeletePartial(
                request.DestinationPath);

            return request with
            {
                ResumeOffsetBytes = 0
            };
        }

        return request with
        {
            ResumeOffsetBytes =
                decision.ShouldResume
                    ? decision.ExistingBytes
                    : 0
        };
    }

    private static ResumeMetadata GetResumeMetadata(
        string path)
    {
        var exists = File.Exists(path);

        return new ResumeMetadata(
            path,
            exists,
            exists
                ? new FileInfo(path).Length
                : 0);
    }

    private static void DeletePartial(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
