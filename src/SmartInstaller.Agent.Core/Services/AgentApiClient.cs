using System.Net.Http.Json;
using SmartInstaller.Agent.Core.Api;
using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public sealed class AgentApiClient(HttpClient httpClient) : IAgentApiClient
{
    public async Task<IReadOnlyList<AgentCatalogItem>> GetCatalogAsync(
        string architecture,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(
            $"api/agent/catalog?architecture={Uri.EscapeDataString(architecture)}",
            cancellationToken);

        return await ReadDataAsync<IReadOnlyList<AgentCatalogItem>>(
            response,
            cancellationToken);
    }

    public async Task<IReadOnlyList<UpdateCheckItem>> CheckUpdatesAsync(
        string architecture,
        IReadOnlyList<MatchedInstalledApplication> applications,
        CancellationToken cancellationToken = default)
    {
        var request = new CheckUpdatesRequest(
            architecture,
            applications.Select(application =>
                new InstalledApplicationRequest(
                    application.CatalogApplication.ApplicationId,
                    application.InstalledApplication.Version!))
                .ToArray());

        var response = await httpClient.PostAsJsonAsync(
            "api/agent/check-updates",
            request,
            cancellationToken);

        return await ReadDataAsync<IReadOnlyList<UpdateCheckItem>>(
            response,
            cancellationToken);
    }

    public async Task<InstallerManifest> GetInstallerManifestAsync(
        Guid installerProfileId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(
            $"api/agent/installer-manifest/{installerProfileId}",
            cancellationToken);

        return await ReadDataAsync<InstallerManifest>(
            response,
            cancellationToken);
    }

    private static async Task<T> ReadDataAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        ApiResponse<T>? result;

        try
        {
            result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<T>>(
                body,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (System.Text.Json.JsonException exception)
        {
            throw new InvalidOperationException(
                $"SmartInstaller API returned an invalid JSON response.{Environment.NewLine}{body}",
                exception);
        }

        if (!response.IsSuccessStatusCode || result is null || !result.Success)
        {
            throw new HttpRequestException(
                result?.Message ??
                $"SmartInstaller API returned {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        return result.Data ??
               throw new InvalidOperationException(
                   "The SmartInstaller API response contained no data.");
    }
}
