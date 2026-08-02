using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Download.Queue;

public interface IConcurrentDownloadManager
{
    Task<ConcurrentDownloadResult> DownloadAsync(
        IReadOnlyCollection<UpdateCheckItem> updates,
        IProgress<DownloadQueueItemProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
