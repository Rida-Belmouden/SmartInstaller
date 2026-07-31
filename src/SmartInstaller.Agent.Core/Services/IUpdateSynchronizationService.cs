using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public interface IUpdateSynchronizationService
{
    Task<UpdateSynchronizationResult> CheckUpdatesAsync(
        IReadOnlyList<InstalledApplication> installedApplications,
        CancellationToken cancellationToken = default);
}
