namespace SmartInstaller.Agent.Core.Download.Queue;

public enum DownloadQueueStatus
{
    Queued,
    Starting,
    Downloading,
    Completed,
    Failed,
    Cancelled
}
