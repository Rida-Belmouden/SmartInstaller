using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public interface IAgentApiClient
{
    Task<IReadOnlyList<AgentCatalogItem>> GetCatalogAsync(
        string architecture,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UpdateCheckItem>> CheckUpdatesAsync(
        string architecture,
        IReadOnlyList<MatchedInstalledApplication> applications,
        CancellationToken cancellationToken = default);
}
