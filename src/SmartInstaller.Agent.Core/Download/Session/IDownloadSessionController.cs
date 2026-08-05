using SmartInstaller.Agent.Core.Download.Queue;
using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Download.Session;

public interface IDownloadSessionController
{
    event EventHandler<DownloadSessionEvent>? SessionEvent;

    bool IsRunning { get; }

    Task<ConcurrentDownloadResult> StartAsync(
        IReadOnlyCollection<UpdateCheckItem> updates,
        CancellationToken cancellationToken = default);

    void CancelAll();
}
