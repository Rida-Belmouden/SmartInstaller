namespace SmartInstaller.Agent.Core.Download.Session;

public sealed record DownloadSessionStatistics(
    int Total,
    int Queued,
    int Active,
    int Paused,
    int Completed,
    int Failed,
    int Cancelled,
    double BytesPerSecond);
