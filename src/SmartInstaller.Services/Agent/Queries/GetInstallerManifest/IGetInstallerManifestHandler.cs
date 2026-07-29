namespace SmartInstaller.Services.Agent.Queries.GetInstallerManifest;

public interface IGetInstallerManifestHandler
{
    Task<GetInstallerManifestResult> HandleAsync(
        GetInstallerManifestQuery query,
        CancellationToken cancellationToken = default);
}
