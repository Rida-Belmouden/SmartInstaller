using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;
using SmartInstaller.Agent.Core.Download.Cache;
using SmartInstaller.Agent.Core.Download.Services;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Agent.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddAgentCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AgentOptions>(
            configuration.GetSection(AgentOptions.SectionName));

        services.Configure<DownloadOptions>(
            configuration.GetSection(DownloadOptions.SectionName));

        services.AddSingleton<
            IApplicationNameNormalizer,
            ApplicationNameNormalizer>();

        services.AddSingleton<
            IInstalledSoftwareScanner,
            InstalledSoftwareScanner>();

        services.AddSingleton<
            ISystemArchitectureDetector,
            SystemArchitectureDetector>();

        services.AddSingleton<
            IApplicationMatcher,
            ApplicationMatcher>();

        services.AddSingleton<
            IUpdateSynchronizationService,
            UpdateSynchronizationService>();

        services.AddSingleton<
            ICachePathProvider,
            CachePathProvider>();

        services.AddHttpClient<
            IAgentApiClient,
            AgentApiClient>((provider, client) =>
        {
            var options = provider
                .GetRequiredService<IOptions<AgentOptions>>()
                .Value;

            client.BaseAddress = new Uri(
                EnsureTrailingSlash(options.ApiBaseUrl));

            client.Timeout = options.RequestTimeout;
        });

        services.AddHttpClient<
            IDownloadManager,
            DownloadManager>((provider, client) =>
        {
            var options = provider
                .GetRequiredService<IOptions<DownloadOptions>>()
                .Value;

            client.Timeout = options.RequestTimeout;
        });

        return services;
    }

    private static string EnsureTrailingSlash(string value)
    {
        return value.EndsWith(
            "/",
            StringComparison.Ordinal)
                ? value
                : value + "/";
    }
}
