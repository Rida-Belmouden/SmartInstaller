using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public interface IUpdateDownloadStateService
{
    Task<UpdateDownloadState?> GetStateAsync(
        UpdateCheckItem update,
        CancellationToken cancellationToken = default);
}
