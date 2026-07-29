namespace SmartInstaller.Services.InstallerProfiles
    .Commands.CreateInstallerProfile;

public interface ICreateInstallerProfileHandler
{
    Task<CreateInstallerProfileResult> HandleAsync(
        CreateInstallerProfileCommand command,
        CancellationToken cancellationToken = default);
}
