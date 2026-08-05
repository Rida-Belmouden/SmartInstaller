using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Download.Queue;

public sealed record DownloadQueueItemResult(
    int Index,
    UpdateCheckItem Update,
    DownloadQueueStatus Status,
    UpdateDownloadResult? DownloadResult,
    string? ErrorMessage)
{
    public DownloadQueueCancellationReason CancellationReason
    {
        get;
        init;
    }

    public bool IsSuccess =>
        Status == DownloadQueueStatus.Completed &&
        DownloadResult?.DownloadResult.IsSuccess == true;
}
