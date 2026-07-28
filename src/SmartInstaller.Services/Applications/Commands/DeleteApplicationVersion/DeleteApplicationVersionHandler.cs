using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;

namespace SmartInstaller.Services.Applications
    .Commands.DeleteApplicationVersion;

public sealed class DeleteApplicationVersionHandler(
    ApplicationDbContext dbContext)
    : IDeleteApplicationVersionHandler
{
    public async Task<DeleteApplicationVersionResult> HandleAsync(
        DeleteApplicationVersionCommand command,
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
            return new DeleteApplicationVersionResult(
                DeleteApplicationVersionStatus.VersionNotFound);
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var wasLatest = targetVersion.IsLatest;
        var updatedAt = DateTime.UtcNow;

        targetVersion.IsActive = false;
        targetVersion.IsLatest = false;
        targetVersion.UpdatedAt = updatedAt;

        if (wasLatest)
        {
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
                var otherLatestVersions =
                    await dbContext.ApplicationVersions
                        .Where(version =>
                            version.SoftwareApplicationId ==
                                targetVersion.SoftwareApplicationId &&
                            version.Id != targetVersion.Id &&
                            version.Id != replacementVersion.Id &&
                            version.IsActive &&
                            version.IsLatest)
                        .ToListAsync(cancellationToken);

                foreach (var version in otherLatestVersions)
                {
                    version.IsLatest = false;
                    version.UpdatedAt = updatedAt;
                }

                replacementVersion.IsLatest = true;
                replacementVersion.UpdatedAt = updatedAt;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new DeleteApplicationVersionResult(
            DeleteApplicationVersionStatus.Success);
    }
}