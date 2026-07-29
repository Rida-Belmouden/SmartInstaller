using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Agent.DTOs;

namespace SmartInstaller.Services.Agent.Queries.GetInstallerManifest;

public sealed class GetInstallerManifestHandler(ApplicationDbContext dbContext)
    : IGetInstallerManifestHandler
{
    public async Task<GetInstallerManifestResult> HandleAsync(
        GetInstallerManifestQuery query,
        CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.InstallerProfiles
            .AsNoTracking()
            .Include(item => item.InstallerType)
            .Include(item => item.Architecture)
            .Include(item => item.ApplicationVersion)
                .ThenInclude(version => version.SoftwareApplication)
            .SingleOrDefaultAsync(
                item =>
                    item.PublicId == query.InstallerProfileId &&
                    item.IsActive &&
                    item.IsEnabled &&
                    item.ApplicationVersion.IsActive &&
                    item.ApplicationVersion.SoftwareApplication.IsActive,
                cancellationToken);

        if (profile is null)
        {
            return new GetInstallerManifestResult(
                GetInstallerManifestStatus.NotFound);
        }

        var manifest = new InstallerManifestDto(
            profile.PublicId,
            profile.ApplicationVersion.SoftwareApplication.PublicId,
            profile.ApplicationVersion.SoftwareApplication.Name,
            profile.ApplicationVersion.PublicId,
            profile.ApplicationVersion.Version,
            profile.InstallerType.Name,
            profile.Architecture.Name,
            profile.DownloadUrl,
            profile.Sha256,
            profile.FileSizeBytes,
            profile.SilentInstallArguments,
            profile.SilentUninstallArguments,
            profile.RequiresAdministrator,
            profile.IsPortable);

        return new GetInstallerManifestResult(
            GetInstallerManifestStatus.Success,
            manifest);
    }
}
