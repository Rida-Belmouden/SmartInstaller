using Microsoft.Extensions.DependencyInjection;
using SmartInstaller.Services.Agent.Queries.CheckUpdates;
using SmartInstaller.Services.Agent.Queries.GetAgentCatalog;
using SmartInstaller.Services.Agent.Queries.GetInstallerManifest;
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
using SmartInstaller.Services.InstallerProfiles.Commands.CreateInstallerProfile;
using SmartInstaller.Services.InstallerProfiles.Commands.DeactivateInstallerProfile;
using SmartInstaller.Services.InstallerProfiles.Commands.UpdateInstallerProfile;
using SmartInstaller.Services.InstallerProfiles.Queries.GetInstallerProfileById;
using SmartInstaller.Services.InstallerProfiles.Queries.GetInstallerProfiles;

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

        services.AddScoped<
            IGetInstallerProfilesHandler,
            GetInstallerProfilesHandler>();

        services.AddScoped<
            IGetInstallerProfileByIdHandler,
            GetInstallerProfileByIdHandler>();

        services.AddScoped<
            ICreateInstallerProfileHandler,
            CreateInstallerProfileHandler>();

        services.AddScoped<
            IUpdateInstallerProfileHandler,
            UpdateInstallerProfileHandler>();

        services.AddScoped<
            IDeactivateInstallerProfileHandler,
            DeactivateInstallerProfileHandler>();


        services.AddScoped<
            IGetAgentCatalogHandler,
            GetAgentCatalogHandler>();

        services.AddScoped<
            ICheckUpdatesHandler,
            CheckUpdatesHandler>();

        services.AddScoped<
            IGetInstallerManifestHandler,
            GetInstallerManifestHandler>();

        return services;
    }
}