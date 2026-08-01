using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;
using SmartInstaller.Agent.Core.Download.Cache;
using SmartInstaller.Agent.Core.Download.Http;
using SmartInstaller.Agent.Core.Download.Retry;
using SmartInstaller.Agent.Core.Download.Services;
using SmartInstaller.Agent.Core.Download.Verification;
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

        services.Configure<RetryOptions>(
            configuration.GetSection(RetryOptions.SectionName));

        services.AddSingleton<IApplicationNameNormalizer, ApplicationNameNormalizer>();
        services.AddSingleton<IInstalledSoftwareScanner, InstalledSoftwareScanner>();
        services.AddSingleton<ISystemArchitectureDetector, SystemArchitectureDetector>();
        services.AddSingleton<IApplicationMatcher, ApplicationMatcher>();
        services.AddSingleton<IUpdateSynchronizationService, UpdateSynchronizationService>();

        services.AddSingleton<ICachePathProvider, CachePathProvider>();
        services.AddSingleton<IFileCacheService, FileCacheService>();
        services.AddSingleton<ISha256Verifier, Sha256Verifier>();
        services.AddSingleton<IRetryPolicy, RetryPolicy>();
        services.AddSingleton<IRetryDelay, SystemRetryDelay>();

        services.AddHttpClient<IAgentApiClient, AgentApiClient>((provider, client) =>
        {
            var options = provider
                .GetRequiredService<IOptions<AgentOptions>>()
                .Value;

            client.BaseAddress = new Uri(
                EnsureTrailingSlash(options.ApiBaseUrl));

            client.Timeout = options.RequestTimeout;
        });

        services.AddHttpClient<IHttpDownloadAttemptExecutor, HttpDownloadAttemptExecutor>((provider, client) =>
        {
            var options = provider
                .GetRequiredService<IOptions<DownloadOptions>>()
                .Value;

            client.Timeout = options.RequestTimeout;
        });

        services.AddTransient<IHttpDownloader, RetryingHttpDownloader>();
        services.AddTransient<IDownloadManager, DownloadManager>();
        services.AddTransient<IUpdateDownloadService, UpdateDownloadService>();

        return services;
    }

    private static string EnsureTrailingSlash(string value) =>
        value.EndsWith("/", StringComparison.Ordinal)
            ? value
            : value + "/";
}
