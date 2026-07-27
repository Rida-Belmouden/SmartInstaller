using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Catalog.DTOs;

namespace SmartInstaller.Services.Catalog.Queries.GetPlatforms;

public sealed class GetPlatformsHandler(
    ApplicationDbContext dbContext)
    : IGetPlatformsHandler
{
    public async Task<IReadOnlyCollection<CatalogItemDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Platforms
            .AsNoTracking()
            .Where(platform => platform.IsActive)
            .OrderBy(platform => platform.Name)
            .Select(platform => new CatalogItemDto(
                platform.PublicId,
                platform.Name,
                platform.Slug,
                platform.Description,
                platform.Applications.Count(application =>
                    application.IsActive)))
            .ToListAsync(cancellationToken);
    }
}