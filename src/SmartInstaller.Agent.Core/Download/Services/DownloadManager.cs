using SmartInstaller.Agent.Core.Download.Models;

namespace SmartInstaller.Agent.Core.Download.Services;

public sealed class DownloadManager : IDownloadManager
{
    private const string FoundationMessage =
        "HTTP downloading is not implemented in the foundation phase.";

    public Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(
            DownloadResult.Failed(FoundationMessage));
    }
}
