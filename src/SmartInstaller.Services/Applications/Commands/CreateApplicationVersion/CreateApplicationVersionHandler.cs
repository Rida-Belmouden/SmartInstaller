using Microsoft.EntityFrameworkCore;
using SmartInstaller.Core.Entities.Catalog;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications
    .Commands.CreateApplicationVersion;

public sealed class CreateApplicationVersionHandler(
    ApplicationDbContext dbContext)
    : ICreateApplicationVersionHandler
{
    public async Task<CreateApplicationVersionResult> HandleAsync(
        CreateApplicationVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedVersion = command.Version.Trim();

        if (string.IsNullOrWhiteSpace(normalizedVersion))
        {
            return new CreateApplicationVersionResult(
                CreateApplicationVersionStatus.InvalidVersion);
        }

        var application = await dbContext.Applications
            .FirstOrDefaultAsync(
                item =>
                    item.PublicId == command.ApplicationPublicId &&
                    item.IsActive,
                cancellationToken);

        if (application is null)
        {
            return new CreateApplicationVersionResult(
                CreateApplicationVersionStatus.ApplicationNotFound);
        }

        var duplicateExists =
            await dbContext.ApplicationVersions.AnyAsync(
                version =>
                    version.SoftwareApplicationId == application.Id &&
                    version.Version == normalizedVersion,
                cancellationToken);

        if (duplicateExists)
        {
            return new CreateApplicationVersionResult(
                CreateApplicationVersionStatus.DuplicateVersion);
        }

        var hasExistingVersions =
            await dbContext.ApplicationVersions.AnyAsync(
                version =>
                    version.SoftwareApplicationId == application.Id &&
                    version.IsActive,
                cancellationToken);

        var shouldBeLatest =
            command.IsLatest || !hasExistingVersions;

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        if (shouldBeLatest)
        {
            var currentLatestVersions =
                await dbContext.ApplicationVersions
                    .Where(version =>
                        version.SoftwareApplicationId ==
                            application.Id &&
                        version.IsLatest)
                    .ToListAsync(cancellationToken);

            foreach (var currentVersion in currentLatestVersions)
            {
                currentVersion.IsLatest = false;
            }
        }

        var applicationVersion = new ApplicationVersion
        {
            SoftwareApplicationId = application.Id,
            Version = normalizedVersion,
            ReleaseDate = command.ReleaseDate,
            IsLatest = shouldBeLatest,
            IsActive = true
        };

        dbContext.ApplicationVersions.Add(applicationVersion);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var dto = new ApplicationVersionDto(
            applicationVersion.PublicId,
            applicationVersion.Version,
            applicationVersion.ReleaseDate,
            applicationVersion.IsLatest,
            applicationVersion.IsActive);

        return new CreateApplicationVersionResult(
            CreateApplicationVersionStatus.Success,
            dto);
    }
}
