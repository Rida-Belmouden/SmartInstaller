using SmartInstaller.Services.InstallerProfiles.DTOs;

namespace SmartInstaller.Services.InstallerProfiles
    .Queries.GetInstallerProfileById;

public sealed record GetInstallerProfileByIdResult(
    GetInstallerProfileByIdStatus Status,
    InstallerProfileDto? InstallerProfile = null);

public enum GetInstallerProfileByIdStatus
{
    Success,
    NotFound
}
