using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public interface IUpdateDownloadService
{
    Task<UpdateDownloadResult> DownloadAsync(
        UpdateCheckItem update,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
