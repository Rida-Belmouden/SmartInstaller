using SmartInstaller.Agent.Core.Installation.Models;
using SmartInstaller.Agent.Core.Installation.Services;
using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public sealed class UpdateInstallationService(
    IInstallerService installerService)
    : IUpdateInstallationService
{
    public async Task<UpdateInstallationResult> InstallAsync(
        UpdateCheckItem update,
        InstallerManifest manifest,
        string installerPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentException.ThrowIfNullOrWhiteSpace(installerPath);

        var installerKind = ParseInstallerKind(
            manifest.InstallerType);

        if (!installerKind.HasValue)
        {
            return CreateFailedResult(
                update,
                manifest,
                installerPath,
                InstallStatus.UnsupportedInstaller,
                $"Installer type '{manifest.InstallerType}' is not supported yet.");
        }

        if (manifest.IsPortable)
        {
            return CreateFailedResult(
                update,
                manifest,
                installerPath,
                InstallStatus.UnsupportedInstaller,
                "Portable packages are not supported by the installer engine yet.");
        }

        var result = await installerService.InstallAsync(
            new InstallRequest(
                installerPath,
                installerKind.Value,
                manifest.SilentInstallArguments,
                manifest.RequiresAdministrator),
            cancellationToken);

        return new UpdateInstallationResult(
            update,
            manifest,
            installerPath,
            result);
    }

    private static InstallerKind? ParseInstallerKind(
        string installerType)
    {
        return installerType.Trim().ToUpperInvariant() switch
        {
            "EXE" => InstallerKind.Exe,
            "MSI" => InstallerKind.Msi,
            _ => null
        };
    }

    private static UpdateInstallationResult CreateFailedResult(
        UpdateCheckItem update,
        InstallerManifest manifest,
        string installerPath,
        InstallStatus status,
        string message)
    {
        return new UpdateInstallationResult(
            update,
            manifest,
            installerPath,
            new InstallResult(
                status,
                null,
                TimeSpan.Zero,
                message));
    }
}
