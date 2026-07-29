using SmartInstaller.Services.InstallerProfiles.DTOs;

namespace SmartInstaller.Services.InstallerProfiles
    .Queries.GetInstallerProfiles;

public interface IGetInstallerProfilesHandler
{
    Task<IReadOnlyList<InstallerProfileDto>> HandleAsync(
        GetInstallerProfilesQuery query,
        CancellationToken cancellationToken = default);
}
