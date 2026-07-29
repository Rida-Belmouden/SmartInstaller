using SmartInstaller.Core.Entities.Installer;
using SmartInstaller.Services.InstallerProfiles.DTOs;

namespace SmartInstaller.Services.InstallerProfiles.Common;

internal static class InstallerProfileMapper
{
    public static InstallerProfileDto Map(InstallerProfile profile)
    {
        return new InstallerProfileDto(
            profile.PublicId,
            profile.ApplicationVersion.PublicId,
            profile.InstallerType.PublicId,
            profile.InstallerType.Name,
            profile.Architecture.PublicId,
            profile.Architecture.Name,
            profile.DownloadUrl,
            profile.Sha256,
            profile.FileSizeBytes,
            profile.SilentInstallArguments,
            profile.SilentUninstallArguments,
            profile.RequiresAdministrator,
            profile.IsPortable,
            profile.IsEnabled,
            profile.IsActive);
    }
}
