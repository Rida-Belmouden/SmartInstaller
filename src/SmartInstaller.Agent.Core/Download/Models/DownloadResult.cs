namespace SmartInstaller.Agent.Core.Download.Models;

public sealed record DownloadResult(
    DownloadStatus Status,
    string? FilePath,
    string? ErrorMessage,
    TimeSpan Duration)
{
    public bool IsSuccess =>
        Status is DownloadStatus.Completed or DownloadStatus.Cached;

    public static DownloadResult Failed(
        string errorMessage,
        TimeSpan duration = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return new DownloadResult(
            DownloadStatus.Failed,
            null,
            errorMessage,
            duration);
    }
}
