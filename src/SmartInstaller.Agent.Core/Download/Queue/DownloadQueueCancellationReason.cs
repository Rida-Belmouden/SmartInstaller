namespace SmartInstaller.Agent.Core.Download.Queue;

public enum DownloadQueueCancellationReason
{
    None,
    PauseItem,
    CancelItem,
    CancelAll
}
