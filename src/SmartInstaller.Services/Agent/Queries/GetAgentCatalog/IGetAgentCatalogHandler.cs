using SmartInstaller.Services.Agent.DTOs;

namespace SmartInstaller.Services.Agent.Queries.GetAgentCatalog;

public interface IGetAgentCatalogHandler
{
    Task<IReadOnlyList<AgentCatalogItemDto>> HandleAsync(
        GetAgentCatalogQuery query,
        CancellationToken cancellationToken = default);
}
