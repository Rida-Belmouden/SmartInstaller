namespace SmartInstaller.Services.InstallerProfiles
    .Commands.DeactivateInstallerProfile;

public sealed record DeactivateInstallerProfileResult(
    DeactivateInstallerProfileStatus Status);

public enum DeactivateInstallerProfileStatus
{
    Success,
    NotFound,
    AlreadyInactive
}
