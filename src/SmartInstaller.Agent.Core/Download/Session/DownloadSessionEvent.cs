using SmartInstaller.Agent.Core.Download.Queue;

namespace SmartInstaller.Agent.Core.Download.Session;

public sealed record DownloadSessionEvent(
    DownloadSessionEventType Type,
    DownloadSessionItemState? Item = null,
    DownloadSessionStatistics? Statistics = null,
    ConcurrentDownloadResult? Result = null,
    string? Message = null);
