using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public interface IUpdateInstallationService
{
    Task<UpdateInstallationResult> InstallAsync(
        UpdateCheckItem update,
        InstallerManifest manifest,
        string installerPath,
        CancellationToken cancellationToken = default);
}
