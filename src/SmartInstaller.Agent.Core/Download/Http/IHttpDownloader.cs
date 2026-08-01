namespace SmartInstaller.Agent.Core.Download.Http;

public interface IHttpDownloader
{
    Task<HttpDownloadResult> DownloadAsync(
        HttpDownloadRequest request,
        CancellationToken cancellationToken = default);
}
