using Microsoft.EntityFrameworkCore;
using SmartInstaller.Core.Entities.Catalog;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications
    .Commands.UpdateApplicationVersion;

public sealed class UpdateApplicationVersionHandler(
    ApplicationDbContext dbContext)
    : IUpdateApplicationVersionHandler
{
    public async Task<UpdateApplicationVersionResult> HandleAsync(
        UpdateApplicationVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedVersion = command.Version.Trim();

        if (string.IsNullOrWhiteSpace(normalizedVersion))
        {
            return new UpdateApplicationVersionResult(
                UpdateApplicationVersionStatus.InvalidVersion);
        }

        var targetVersion = await dbContext.ApplicationVersions
            .FirstOrDefaultAsync(
                version =>
                    version.PublicId == command.VersionPublicId &&
                    version.IsActive,
                cancellationToken);

        if (targetVersion is null)
        {
            return new UpdateApplicationVersionResult(
                UpdateApplicationVersionStatus.VersionNotFound);
        }

        var duplicateExists =
            await dbContext.ApplicationVersions.AnyAsync(
                version =>
                    version.Id != targetVersion.Id &&
                    version.SoftwareApplicationId ==
                        targetVersion.SoftwareApplicationId &&
                    version.Version == normalizedVersion &&
                    version.IsActive,
                cancellationToken);

        if (duplicateExists)
        {
            return new UpdateApplicationVersionResult(
                UpdateApplicationVersionStatus.DuplicateVersion);
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var updatedAt = DateTime.UtcNow;

        if (command.IsLatest && !targetVersion.IsLatest)
        {
            var currentLatestVersions =
                await dbContext.ApplicationVersions
                    .Where(version =>
                        version.SoftwareApplicationId ==
                            targetVersion.SoftwareApplicationId &&
                        version.Id != targetVersion.Id &&
                        version.IsLatest &&
                        version.IsActive)
                    .ToListAsync(cancellationToken);

            foreach (var currentVersion in currentLatestVersions)
            {
                currentVersion.IsLatest = false;
                currentVersion.UpdatedAt = updatedAt;
            }
        }

        targetVersion.Version = normalizedVersion;
        targetVersion.ReleaseDate = command.ReleaseDate;
        targetVersion.IsLatest = command.IsLatest;
        targetVersion.UpdatedAt = updatedAt;

        await EnsureLatestVersionExistsAsync(
            targetVersion,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new UpdateApplicationVersionResult(
            UpdateApplicationVersionStatus.Success,
            Map(targetVersion));
    }

    private async Task EnsureLatestVersionExistsAsync(
        ApplicationVersion targetVersion,
        CancellationToken cancellationToken)
    {
        if (targetVersion.IsLatest)
        {
            return;
        }

        var anotherLatestExists =
            await dbContext.ApplicationVersions.AnyAsync(
                version =>
                    version.SoftwareApplicationId ==
                        targetVersion.SoftwareApplicationId &&
                    version.Id != targetVersion.Id &&
                    version.IsActive &&
                    version.IsLatest,
                cancellationToken);

        if (anotherLatestExists)
        {
            return;
        }

        var replacementVersion =
            await dbContext.ApplicationVersions
                .Where(version =>
                    version.SoftwareApplicationId ==
                        targetVersion.SoftwareApplicationId &&
                    version.Id != targetVersion.Id &&
                    version.IsActive)
                .OrderByDescending(version => version.ReleaseDate)
                .ThenByDescending(version => version.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        if (replacementVersion is not null)
        {
            replacementVersion.IsLatest = true;
            replacementVersion.UpdatedAt = DateTime.UtcNow;
            return;
        }

        // لا يمكن ترك التطبيق بدون إصدار أحدث
        // إذا كان هذا هو الإصدار النشط الوحيد.
        targetVersion.IsLatest = true;
    }

    private static ApplicationVersionDto Map(
        ApplicationVersion version)
    {
        return new ApplicationVersionDto(
            version.PublicId,
            version.Version,
            version.ReleaseDate,
            version.IsLatest,
            version.IsActive);
    }
}