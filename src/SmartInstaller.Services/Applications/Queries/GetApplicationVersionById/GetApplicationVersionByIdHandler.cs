using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications
    .Queries.GetApplicationVersionById;

public sealed class GetApplicationVersionByIdHandler(
    ApplicationDbContext dbContext)
    : IGetApplicationVersionByIdHandler
{
    public Task<ApplicationVersionDto?> HandleAsync(
        GetApplicationVersionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ApplicationVersions
            .AsNoTracking()
            .Where(version =>
                version.PublicId == query.VersionPublicId &&
                version.IsActive)
            .Select(version => new ApplicationVersionDto(
                version.PublicId,
                version.Version,
                version.ReleaseDate,
                version.IsLatest,
                version.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }
}