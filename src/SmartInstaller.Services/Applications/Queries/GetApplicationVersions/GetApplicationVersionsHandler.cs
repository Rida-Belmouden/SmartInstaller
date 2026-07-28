using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications
    .Queries.GetApplicationVersions;

public sealed class GetApplicationVersionsHandler(
    ApplicationDbContext dbContext)
    : IGetApplicationVersionsHandler
{
    public async Task<IReadOnlyList<ApplicationVersionDto>?> HandleAsync(
        GetApplicationVersionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var applicationExists =
            await dbContext.Applications
                .AsNoTracking()
                .AnyAsync(
                    application =>
                        application.PublicId ==
                        query.ApplicationPublicId &&
                        application.IsActive,
                    cancellationToken);

        if (!applicationExists)
        {
            return null;
        }

        return await dbContext.ApplicationVersions
            .AsNoTracking()
            .Where(version =>
                version.SoftwareApplication.PublicId ==
                query.ApplicationPublicId)
            .OrderByDescending(version => version.IsLatest)
            .ThenByDescending(version => version.ReleaseDate)
            .ThenByDescending(version => version.CreatedAt)
            .Select(version => new ApplicationVersionDto(
                version.PublicId,
                version.Version,
                version.ReleaseDate,
                version.IsLatest,
                version.IsActive))
            .ToListAsync(cancellationToken);
    }
}