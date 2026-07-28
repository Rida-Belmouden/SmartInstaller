using Microsoft.Extensions.DependencyInjection;
using SmartInstaller.Services.Applications.Commands.CreateApplicationVersion;
using SmartInstaller.Services.Applications.Commands.DeleteApplicationVersion;
using SmartInstaller.Services.Applications.Commands.SetLatestApplicationVersion;
using SmartInstaller.Services.Applications.Commands.UpdateApplicationVersion;
using SmartInstaller.Services.Applications.Queries.GetApplicationById;
using SmartInstaller.Services.Applications.Queries.GetApplications;
using SmartInstaller.Services.Applications.Queries.GetApplicationVersionById;
using SmartInstaller.Services.Applications.Queries.GetApplicationVersions;
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

        services.AddScoped<
            IGetApplicationVersionsHandler,
            GetApplicationVersionsHandler>();

        services.AddScoped<
            ICreateApplicationVersionHandler,
            CreateApplicationVersionHandler>();

        services.AddScoped<
            IGetApplicationVersionByIdHandler,
            GetApplicationVersionByIdHandler>();

        services.AddScoped<
            ISetLatestApplicationVersionHandler,
            SetLatestApplicationVersionHandler>();

        services.AddScoped<
            IUpdateApplicationVersionHandler,
            UpdateApplicationVersionHandler>();

        services.AddScoped<
            IDeleteApplicationVersionHandler,
            DeleteApplicationVersionHandler>();

        return services;
    }
}