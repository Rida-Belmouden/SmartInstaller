using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Agent.DTOs;

namespace SmartInstaller.Services.Agent.Queries.CheckUpdates;

public sealed class CheckUpdatesHandler(ApplicationDbContext dbContext)
    : ICheckUpdatesHandler
{
    public async Task<IReadOnlyList<UpdateCheckItemDto>> HandleAsync(
        CheckUpdatesQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Applications.Count == 0)
        {
            return [];
        }

        var applicationIds = query.Applications
            .Select(application => application.ApplicationId)
            .Distinct()
            .ToList();

        var applications = await dbContext.Applications
            .AsNoTracking()
            .AsSplitQuery()
            .Include(application => application.Versions)
                .ThenInclude(version => version.InstallerProfiles)
                    .ThenInclude(profile => profile.Architecture)
            .Where(application =>
                application.IsActive &&
                applicationIds.Contains(application.PublicId))
            .ToListAsync(cancellationToken);

        var applicationsById = applications.ToDictionary(
            application => application.PublicId);

        var result = new List<UpdateCheckItemDto>();

        foreach (var installed in query.Applications)
        {
            if (!applicationsById.TryGetValue(
                    installed.ApplicationId,
                    out var application))
            {
                continue;
            }

            var latestVersion = application.Versions
                .Where(version => version.IsActive && version.IsLatest)
                .OrderByDescending(version => version.ReleaseDate)
                .FirstOrDefault();

            if (latestVersion is null)
            {
                continue;
            }

            var profile = latestVersion.InstallerProfiles
                .Where(candidate =>
                    candidate.IsActive &&
                    candidate.IsEnabled &&
                    MatchesArchitecture(
                        candidate.Architecture.Name,
                        query.Architecture))
                .OrderBy(candidate =>
                    candidate.Architecture.Name.Equals(
                        query.Architecture,
                        StringComparison.OrdinalIgnoreCase)
                        ? 0
                        : 1)
                .ThenBy(candidate => candidate.Id)
                .FirstOrDefault();

            var updateAvailable = IsNewerVersion(
                latestVersion.Version,
                installed.InstalledVersion);

            result.Add(new UpdateCheckItemDto(
                application.PublicId,
                application.Name,
                installed.InstalledVersion,
                latestVersion.Version,
                updateAvailable,
                updateAvailable ? profile?.PublicId : null));
        }

        return result;
    }

    private static bool MatchesArchitecture(
        string profileArchitecture,
        string requestedArchitecture)
    {
        return profileArchitecture.Equals("Any", StringComparison.OrdinalIgnoreCase) ||
               profileArchitecture.Equals(
                   requestedArchitecture,
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNewerVersion(
        string latestVersion,
        string installedVersion)
    {
        if (Version.TryParse(latestVersion, out var latest) &&
            Version.TryParse(installedVersion, out var installed))
        {
            return latest.CompareTo(installed) > 0;
        }

        return !latestVersion.Equals(
            installedVersion,
            StringComparison.OrdinalIgnoreCase);
    }
}
