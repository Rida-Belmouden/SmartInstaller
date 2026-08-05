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

    bool PauseItem(UpdateCheckItem update);

    bool ResumeItem(UpdateCheckItem update);

    bool CancelItem(UpdateCheckItem update);

    Task<bool> DiscardPartialDownloadAsync(
        UpdateCheckItem update,
        CancellationToken cancellationToken = default);

    void CancelAll();
}
