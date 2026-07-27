using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Catalog.DTOs;

namespace SmartInstaller.Services.Catalog.Queries.GetCategories;

public sealed class GetCategoriesHandler(
    ApplicationDbContext dbContext)
    : IGetCategoriesHandler
{
    public async Task<IReadOnlyCollection<CatalogItemDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new CatalogItemDto(
                category.PublicId,
                category.Name,
                category.Slug,
                category.Description,
                category.Applications.Count(application =>
                    application.IsActive)))
            .ToListAsync(cancellationToken);
    }
}