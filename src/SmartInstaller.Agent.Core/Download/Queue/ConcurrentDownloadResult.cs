namespace SmartInstaller.Agent.Core.Download.Queue;

public sealed record ConcurrentDownloadResult(
    IReadOnlyList<DownloadQueueItemResult> Items)
{
    public int TotalCount => Items.Count;

    public int CompletedCount =>
        Items.Count(item => item.IsSuccess);

    public int FailedCount =>
        Items.Count(item =>
            item.Status == DownloadQueueStatus.Failed);

    public int CancelledCount =>
        Items.Count(item =>
            item.Status == DownloadQueueStatus.Cancelled);
}
