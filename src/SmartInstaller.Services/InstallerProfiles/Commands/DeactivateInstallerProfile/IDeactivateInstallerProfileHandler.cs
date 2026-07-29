namespace SmartInstaller.Services.InstallerProfiles
    .Commands.DeactivateInstallerProfile;

public interface IDeactivateInstallerProfileHandler
{
    Task<DeactivateInstallerProfileResult> HandleAsync(
        DeactivateInstallerProfileCommand command,
        CancellationToken cancellationToken = default);
}
