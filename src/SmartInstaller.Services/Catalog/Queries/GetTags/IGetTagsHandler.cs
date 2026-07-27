using SmartInstaller.Services.Catalog.DTOs;

namespace SmartInstaller.Services.Catalog.Queries.GetTags;

public interface IGetTagsHandler
{
    Task<IReadOnlyCollection<CatalogItemDto>> HandleAsync(
        CancellationToken cancellationToken = default);
}