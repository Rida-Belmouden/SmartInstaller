namespace SmartInstaller.Agent.Core.Download.Http;
public interface IHttpDownloadAttemptExecutor
{
    Task<HttpDownloadResult> ExecuteAsync(HttpDownloadRequest request, CancellationToken cancellationToken = default);
}
