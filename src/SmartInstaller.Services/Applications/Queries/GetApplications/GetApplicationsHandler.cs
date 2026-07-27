using Microsoft.EntityFrameworkCore;
using SmartInstaller.Core.Entities.Catalog;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.Applications.DTOs;
using SmartInstaller.Services.Common.Models;

namespace SmartInstaller.Services.Applications.Queries.GetApplications;

public sealed class GetApplicationsHandler(
    ApplicationDbContext dbContext)
    : IGetApplicationsHandler
{
    public async Task<PagedResult<ApplicationListItemDto>> HandleAsync(
        GetApplicationsQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<SoftwareApplication> applications =
            dbContext.Applications
                .AsNoTracking()
                .Where(application => application.IsActive);

        applications = ApplyFilters(applications, query);
        applications = ApplySorting(applications, query);

        var totalItems = await applications.CountAsync(
            cancellationToken);

        var items = await applications
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(application => new ApplicationListItemDto(
                application.PublicId,
                application.Name,
                application.Slug,
                application.Description,
                application.IconUrl,
                application.Category.Name,
                application.Publisher.Name,
                application.Platform.Name,
                application.Versions
                    .Where(version =>
                        version.IsActive &&
                        version.IsLatest)
                    .Select(version => version.Version)
                    .FirstOrDefault(),
                application.IsFeatured))
            .ToListAsync(cancellationToken);

        return PagedResult<ApplicationListItemDto>.Create(
            items,
            query.Page,
            query.PageSize,
            totalItems);
    }

    private static IQueryable<SoftwareApplication> ApplyFilters(
        IQueryable<SoftwareApplication> applications,
        GetApplicationsQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            applications = applications.Where(application =>
                application.Name.Contains(search) ||
                application.Slug.Contains(search) ||
                (application.Description != null &&
                 application.Description.Contains(search)) ||
                application.Publisher.Name.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var category = query.Category.Trim();

            applications = applications.Where(application =>
                application.Category.Slug == category ||
                application.Category.Name == category);
        }

        if (!string.IsNullOrWhiteSpace(query.Platform))
        {
            var platform = query.Platform.Trim();

            applications = applications.Where(application =>
                application.Platform.Slug == platform ||
                application.Platform.Name == platform);
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var tag = query.Tag.Trim();

            applications = applications.Where(application =>
                application.ApplicationTags.Any(applicationTag =>
                    applicationTag.Tag.Slug == tag ||
                    applicationTag.Tag.Name == tag));
        }

        if (query.Featured.HasValue)
        {
            applications = applications.Where(application =>
                application.IsFeatured == query.Featured.Value);
        }

        return applications;
    }

    private static IQueryable<SoftwareApplication> ApplySorting(
        IQueryable<SoftwareApplication> applications,
        GetApplicationsQuery query)
    {
        var descending = string.Equals(
            query.SortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        return query.SortBy.Trim().ToLowerInvariant() switch
        {
            "publisher" => descending
                ? applications.OrderByDescending(
                    application => application.Publisher.Name)
                : applications.OrderBy(
                    application => application.Publisher.Name),

            "category" => descending
                ? applications.OrderByDescending(
                    application => application.Category.Name)
                : applications.OrderBy(
                    application => application.Category.Name),

            "createdat" => descending
                ? applications.OrderByDescending(
                    application => application.CreatedAt)
                : applications.OrderBy(
                    application => application.CreatedAt),

            _ => descending
                ? applications.OrderByDescending(
                    application => application.Name)
                : applications.OrderBy(
                    application => application.Name)
        };
    }
}