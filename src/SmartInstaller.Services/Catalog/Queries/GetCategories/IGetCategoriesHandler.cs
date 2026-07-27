using SmartInstaller.Services.Catalog.DTOs;

namespace SmartInstaller.Services.Catalog.Queries.GetCategories;

public interface IGetCategoriesHandler
{
    Task<IReadOnlyCollection<CatalogItemDto>> HandleAsync(
        CancellationToken cancellationToken = default);
}