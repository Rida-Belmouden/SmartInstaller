namespace SmartInstaller.Agent.Core.Download.Models;

public sealed record DownloadResult(
    DownloadStatus Status,
    string? FilePath,
    string? ErrorMessage,
    TimeSpan Duration)
{
    public bool IsSuccess =>
        Status is DownloadStatus.Completed or DownloadStatus.Cached;

    public static DownloadResult Completed(
        string filePath,
        TimeSpan duration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return new DownloadResult(
            DownloadStatus.Completed,
            filePath,
            null,
            duration);
    }

    public static DownloadResult Cached(
        string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return new DownloadResult(
            DownloadStatus.Cached,
            filePath,
            null,
            TimeSpan.Zero);
    }

    public static DownloadResult Cancelled(
        TimeSpan duration)
    {
        return new DownloadResult(
            DownloadStatus.Cancelled,
            null,
            "The download was cancelled.",
            duration);
    }

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
