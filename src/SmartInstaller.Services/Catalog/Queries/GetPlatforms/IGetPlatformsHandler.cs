using SmartInstaller.Services.Catalog.DTOs;

namespace SmartInstaller.Services.Catalog.Queries.GetPlatforms;

public interface IGetPlatformsHandler
{
    Task<IReadOnlyCollection<CatalogItemDto>> HandleAsync(
        CancellationToken cancellationToken = default);
}