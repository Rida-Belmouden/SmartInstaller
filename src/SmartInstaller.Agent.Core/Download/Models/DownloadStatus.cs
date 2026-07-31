namespace SmartInstaller.Agent.Core.Download.Models;

public enum DownloadStatus
{
    Pending,
    Downloading,
    Completed,
    Cancelled,
    Failed,
    VerificationFailed,
    Cached
}
