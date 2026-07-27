using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Catalog.DTOs;

namespace SmartInstaller.Services.Catalog.Queries.GetTags;

public sealed class GetTagsHandler(
    ApplicationDbContext dbContext)
    : IGetTagsHandler
{
    public async Task<IReadOnlyCollection<CatalogItemDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Tags
            .AsNoTracking()
            .Where(tag => tag.IsActive)
            .OrderBy(tag => tag.Name)
            .Select(tag => new CatalogItemDto(
                tag.PublicId,
                tag.Name,
                tag.Slug,
                tag.Description,
                tag.ApplicationTags.Count(applicationTag =>
                    applicationTag.SoftwareApplication.IsActive)))
            .ToListAsync(cancellationToken);
    }
}