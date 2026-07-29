namespace SmartInstaller.Services.InstallerProfiles
    .Commands.UpdateInstallerProfile;

public interface IUpdateInstallerProfileHandler
{
    Task<UpdateInstallerProfileResult> HandleAsync(
        UpdateInstallerProfileCommand command,
        CancellationToken cancellationToken = default);
}
