using SmartInstaller.Services.InstallerProfiles.DTOs;

namespace SmartInstaller.Services.InstallerProfiles
    .Commands.UpdateInstallerProfile;

public sealed record UpdateInstallerProfileResult(
    UpdateInstallerProfileStatus Status,
    InstallerProfileDto? InstallerProfile = null);

public enum UpdateInstallerProfileStatus
{
    Success,
    InstallerProfileNotFound,
    InstallerTypeNotFound,
    ArchitectureNotFound,
    DuplicateInstallerProfile,
    InvalidDownloadUrl,
    InvalidSha256,
    InvalidFileSize
}
