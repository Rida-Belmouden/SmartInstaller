namespace SmartInstaller.Services.InstallerProfiles
    .Queries.GetInstallerProfileById;

public interface IGetInstallerProfileByIdHandler
{
    Task<GetInstallerProfileByIdResult> HandleAsync(
        GetInstallerProfileByIdQuery query,
        CancellationToken cancellationToken = default);
}
