using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Agent.DTOs;

namespace SmartInstaller.Services.Agent.Queries.GetAgentCatalog;

public sealed class GetAgentCatalogHandler(ApplicationDbContext dbContext)
    : IGetAgentCatalogHandler
{
    public async Task<IReadOnlyList<AgentCatalogItemDto>> HandleAsync(
        GetAgentCatalogQuery query,
        CancellationToken cancellationToken = default)
    {
        var architecture = query.Architecture?.Trim();

        var applications = await dbContext.Applications
            .AsNoTracking()
            .Include(application => application.Publisher)
            .Include(application => application.Versions)
                .ThenInclude(version => version.InstallerProfiles)
                    .ThenInclude(profile => profile.InstallerType)
            .Include(application => application.Versions)
                .ThenInclude(version => version.InstallerProfiles)
                    .ThenInclude(profile => profile.Architecture)
            .Where(application => application.IsActive)
            .OrderBy(application => application.Name)
            .ToListAsync(cancellationToken);

        var result = new List<AgentCatalogItemDto>();

        foreach (var application in applications)
        {
            var latestVersion = application.Versions
                .Where(version => version.IsActive && version.IsLatest)
                .OrderByDescending(version => version.ReleaseDate)
                .FirstOrDefault();

            if (latestVersion is null)
            {
                continue;
            }

            var profiles = latestVersion.InstallerProfiles
                .Where(profile =>
                    profile.IsActive &&
                    profile.IsEnabled &&
                    MatchesArchitecture(profile.Architecture.Name, architecture))
                .OrderBy(profile => profile.Architecture.Name)
                .ThenBy(profile => profile.InstallerType.Name)
                .Select(profile => new AgentInstallerOptionDto(
                    profile.PublicId,
                    profile.InstallerType.Name,
                    profile.Architecture.Name,
                    profile.FileSizeBytes,
                    profile.RequiresAdministrator,
                    profile.IsPortable))
                .ToList();

            if (profiles.Count == 0)
            {
                continue;
            }

            result.Add(new AgentCatalogItemDto(
                application.PublicId,
                application.Name,
                application.Publisher.Name,
                latestVersion.PublicId,
                latestVersion.Version,
                latestVersion.ReleaseDate,
                profiles));
        }

        return result;
    }

    private static bool MatchesArchitecture(
        string profileArchitecture,
        string? requestedArchitecture)
    {
        return string.IsNullOrWhiteSpace(requestedArchitecture) ||
               profileArchitecture.Equals("Any", StringComparison.OrdinalIgnoreCase) ||
               profileArchitecture.Equals(
                   requestedArchitecture,
                   StringComparison.OrdinalIgnoreCase);
    }
}
