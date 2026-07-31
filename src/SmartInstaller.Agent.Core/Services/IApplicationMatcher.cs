using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public interface IApplicationMatcher
{
    IReadOnlyList<MatchedInstalledApplication> Match(
        IReadOnlyList<InstalledApplication> installedApplications,
        IReadOnlyList<AgentCatalogItem> catalog);
}
