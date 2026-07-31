using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public sealed class ApplicationMatcher(IApplicationNameNormalizer normalizer)
    : IApplicationMatcher
{
    public IReadOnlyList<MatchedInstalledApplication> Match(
        IReadOnlyList<InstalledApplication> installedApplications,
        IReadOnlyList<AgentCatalogItem> catalog)
    {
        var catalogLookup = catalog
            .GroupBy(item => normalizer.Normalize(item.Name))
            .ToDictionary(group => group.Key, group => group.ToArray());

        var matches = new List<MatchedInstalledApplication>();

        foreach (var installed in installedApplications)
        {
            if (string.IsNullOrWhiteSpace(installed.Version) ||
                !catalogLookup.TryGetValue(installed.NormalizedName, out var candidates))
            {
                continue;
            }

            var match = SelectBestCandidate(installed, candidates);

            if (match is not null)
            {
                matches.Add(new MatchedInstalledApplication(installed, match));
            }
        }

        return matches
            .GroupBy(item => item.CatalogApplication.ApplicationId)
            .Select(group => group.First())
            .ToArray();
    }

    private AgentCatalogItem? SelectBestCandidate(
        InstalledApplication installed,
        IReadOnlyList<AgentCatalogItem> candidates)
    {
        if (candidates.Count == 1)
        {
            return candidates[0];
        }

        var installedPublisher = normalizer.Normalize(installed.Publisher ?? string.Empty);

        return candidates.FirstOrDefault(candidate =>
                   normalizer.Normalize(candidate.Publisher) == installedPublisher)
               ?? candidates.FirstOrDefault();
    }
}
