using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.InstallerProfiles.Common;
using SmartInstaller.Services.InstallerProfiles.DTOs;

namespace SmartInstaller.Services.InstallerProfiles
    .Queries.GetInstallerProfiles;

public sealed class GetInstallerProfilesHandler(
    ApplicationDbContext dbContext)
    : IGetInstallerProfilesHandler
{
    public async Task<IReadOnlyList<InstallerProfileDto>> HandleAsync(
        GetInstallerProfilesQuery query,
        CancellationToken cancellationToken = default)
    {
        var profiles = dbContext.InstallerProfiles
            .AsNoTracking()
            .Include(x => x.ApplicationVersion)
            .Include(x => x.InstallerType)
            .Include(x => x.Architecture)
            .AsQueryable();

        if (!query.IncludeInactive)
        {
            profiles = profiles.Where(x => x.IsActive);
        }

        if (query.ApplicationVersionPublicId.HasValue)
        {
            profiles = profiles.Where(x =>
                x.ApplicationVersion.PublicId ==
                query.ApplicationVersionPublicId.Value);
        }

        if (query.InstallerTypePublicId.HasValue)
        {
            profiles = profiles.Where(x =>
                x.InstallerType.PublicId ==
                query.InstallerTypePublicId.Value);
        }

        if (query.ArchitecturePublicId.HasValue)
        {
            profiles = profiles.Where(x =>
                x.Architecture.PublicId ==
                query.ArchitecturePublicId.Value);
        }

        if (query.IsEnabled.HasValue)
        {
            profiles = profiles.Where(x =>
                x.IsEnabled == query.IsEnabled.Value);
        }

        var entities = await profiles
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities
            .Select(InstallerProfileMapper.Map)
            .ToList();
    }
}
