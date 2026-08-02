using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Download.Queue;

public sealed record DownloadQueueItemProgress(
    int Index,
    UpdateCheckItem Update,
    DownloadQueueStatus Status,
    DownloadProgress? Progress,
    string? Message);
