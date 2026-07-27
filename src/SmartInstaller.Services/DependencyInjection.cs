using Microsoft.Extensions.DependencyInjection;
using SmartInstaller.Services.Applications.Queries.GetApplicationById;
using SmartInstaller.Services.Applications.Queries.GetApplications;
using SmartInstaller.Services.Catalog.Queries.GetCategories;
using SmartInstaller.Services.Catalog.Queries.GetPlatforms;
using SmartInstaller.Services.Catalog.Queries.GetTags;

namespace SmartInstaller.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<
            IGetApplicationsHandler,
            GetApplicationsHandler>();

        services.AddScoped<
            IGetApplicationByIdHandler,
            GetApplicationByIdHandler>();

        services.AddScoped<
            IGetCategoriesHandler,
            GetCategoriesHandler>();

        services.AddScoped<
            IGetPlatformsHandler,
            GetPlatformsHandler>();

        services.AddScoped<
            IGetTagsHandler,
            GetTagsHandler>();

        return services;
    }
}