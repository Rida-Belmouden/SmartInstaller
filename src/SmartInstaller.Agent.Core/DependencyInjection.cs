using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;
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

        services.AddSingleton<IApplicationNameNormalizer, ApplicationNameNormalizer>();
        services.AddSingleton<IInstalledSoftwareScanner, InstalledSoftwareScanner>();
        services.AddSingleton<ISystemArchitectureDetector, SystemArchitectureDetector>();
        services.AddSingleton<IApplicationMatcher, ApplicationMatcher>();
        services.AddSingleton<IUpdateSynchronizationService, UpdateSynchronizationService>();

        services.AddHttpClient<IAgentApiClient, AgentApiClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<AgentOptions>>().Value;
            client.BaseAddress = new Uri(EnsureTrailingSlash(options.ApiBaseUrl));
            client.Timeout = options.RequestTimeout;
        });

        return services;
    }

    private static string EnsureTrailingSlash(string value) =>
        value.EndsWith("/", StringComparison.Ordinal) ? value : value + "/";
}
