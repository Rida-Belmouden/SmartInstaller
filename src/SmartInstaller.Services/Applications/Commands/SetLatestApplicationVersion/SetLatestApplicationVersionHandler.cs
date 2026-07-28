using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications
    .Commands.SetLatestApplicationVersion;

public sealed class SetLatestApplicationVersionHandler(
    ApplicationDbContext dbContext)
    : ISetLatestApplicationVersionHandler
{
    public async Task<SetLatestApplicationVersionResult> HandleAsync(
        SetLatestApplicationVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        var targetVersion = await dbContext.ApplicationVersions
            .FirstOrDefaultAsync(
                version =>
                    version.PublicId == command.VersionPublicId &&
                    version.IsActive,
                cancellationToken);

        if (targetVersion is null)
        {
            return new SetLatestApplicationVersionResult(
                SetLatestApplicationVersionStatus.VersionNotFound);
        }

        if (targetVersion.IsLatest)
        {
            return new SetLatestApplicationVersionResult(
                SetLatestApplicationVersionStatus.AlreadyLatest,
                Map(targetVersion));
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var currentLatestVersions =
            await dbContext.ApplicationVersions
                .Where(version =>
                    version.SoftwareApplicationId ==
                        targetVersion.SoftwareApplicationId &&
                    version.IsLatest)
                .ToListAsync(cancellationToken);

        var updatedAt = DateTime.UtcNow;

        foreach (var version in currentLatestVersions)
        {
            version.IsLatest = false;
            version.UpdatedAt = updatedAt;
        }

        targetVersion.IsLatest = true;
        targetVersion.UpdatedAt = updatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new SetLatestApplicationVersionResult(
            SetLatestApplicationVersionStatus.Success,
            Map(targetVersion));
    }

    private static ApplicationVersionDto Map(
        Core.Entities.Catalog.ApplicationVersion version)
    {
        return new ApplicationVersionDto(
            version.PublicId,
            version.Version,
            version.ReleaseDate,
            version.IsLatest,
            version.IsActive);
    }
}