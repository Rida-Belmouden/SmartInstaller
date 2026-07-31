using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public sealed class UpdateSynchronizationService(
    IAgentApiClient apiClient,
    IApplicationMatcher matcher,
    ISystemArchitectureDetector architectureDetector)
    : IUpdateSynchronizationService
{
    public async Task<UpdateSynchronizationResult> CheckUpdatesAsync(
        IReadOnlyList<InstalledApplication> installedApplications,
        CancellationToken cancellationToken = default)
    {
        var architecture = architectureDetector.Detect();
        var catalog = await apiClient.GetCatalogAsync(architecture, cancellationToken);
        var matches = matcher.Match(installedApplications, catalog);

        if (matches.Count == 0)
        {
            return new UpdateSynchronizationResult(
                [], installedApplications.Count, 0);
        }

        var updates = await apiClient.CheckUpdatesAsync(
            architecture,
            matches,
            cancellationToken);

        return new UpdateSynchronizationResult(
            updates,
            installedApplications.Count,
            matches.Count);
    }
}
