using SmartInstaller.Agent.Core.Installation.Models;

namespace SmartInstaller.Agent.Core.Installation.Services;

public interface IInstallerService
{
    Task<InstallResult> InstallAsync(
        InstallRequest request,
        CancellationToken cancellationToken = default);
}
