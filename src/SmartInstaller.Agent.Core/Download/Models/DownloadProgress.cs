namespace SmartInstaller.Agent.Core.Download.Models;

public sealed record DownloadProgress(
    long BytesReceived,
    long? TotalBytes,
    double? Percentage,
    double BytesPerSecond);
