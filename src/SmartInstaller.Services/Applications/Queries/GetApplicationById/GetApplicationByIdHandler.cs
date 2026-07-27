using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications.Queries.GetApplicationById;

public sealed class GetApplicationByIdHandler(
    ApplicationDbContext dbContext)
    : IGetApplicationByIdHandler
{
    public async Task<ApplicationDetailsDto?> HandleAsync(
        GetApplicationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Applications
            .AsNoTracking()
            .Where(application =>
                application.PublicId == query.PublicId &&
                application.IsActive)
            .Select(application => new ApplicationDetailsDto(
                application.PublicId,
                application.Name,
                application.Slug,
                application.Description,
                application.Website,
                application.IconUrl,
                application.IsFeatured,
                application.Category.Name,
                application.Publisher.Name,
                application.Platform.Name,
                application.ApplicationTags
                    .Where(applicationTag =>
                        applicationTag.Tag.IsActive)
                    .OrderBy(applicationTag =>
                        applicationTag.Tag.Name)
                    .Select(applicationTag =>
                        applicationTag.Tag.Name)
                    .ToList(),
                application.Versions
                    .Where(version => version.IsActive)
                    .OrderByDescending(version =>
                        version.IsLatest)
                    .ThenByDescending(version =>
                        version.ReleaseDate)
                    .Select(version =>
                        new ApplicationVersionDto(
                            version.PublicId,
                            version.Version,
                            version.ReleaseDate,
                            version.IsLatest))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}