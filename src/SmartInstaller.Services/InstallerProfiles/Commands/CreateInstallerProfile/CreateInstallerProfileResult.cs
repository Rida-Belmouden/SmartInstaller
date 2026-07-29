using SmartInstaller.Services.InstallerProfiles.DTOs;

namespace SmartInstaller.Services.InstallerProfiles
    .Commands.CreateInstallerProfile;

public sealed record CreateInstallerProfileResult(
    CreateInstallerProfileStatus Status,
    InstallerProfileDto? InstallerProfile = null);

public enum CreateInstallerProfileStatus
{
    Success,
    ApplicationVersionNotFound,
    InstallerTypeNotFound,
    ArchitectureNotFound,
    DuplicateInstallerProfile,
    InvalidDownloadUrl,
    InvalidSha256,
    InvalidFileSize
}
