namespace SmartInstaller.Agent.Core.Download.Session;

public enum DownloadSessionEventType
{
    SessionStarted,
    ItemUpdated,
    SnapshotUpdated,
    SessionCompleted,
    SessionCancelled,
    SessionFailed
}
