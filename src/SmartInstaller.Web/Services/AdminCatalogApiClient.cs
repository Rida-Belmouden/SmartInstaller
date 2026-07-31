using System.Net.Http.Json;
using System.Text.Json;
using SmartInstaller.Web.Models.AdminCatalog;

namespace SmartInstaller.Web.Services;

public sealed class AdminCatalogApiClient(HttpClient httpClient)
    : IAdminCatalogApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<ApplicationListItem>> GetApplicationsAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        var url = "api/applications?page=1&pageSize=100";

        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search.Trim())}";
        }

        var response = await SendAsync<
            PagedResult<ApplicationListItem>>(
                HttpMethod.Get,
                url,
                null,
                cancellationToken);

        return response.Data?.Items
            ?? Array.Empty<ApplicationListItem>();
    }

    public async Task<ApplicationDetails?> GetApplicationAsync(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var response = await SendAsync<ApplicationDetails>(
            HttpMethod.Get,
            $"api/applications/{publicId}",
            null,
            cancellationToken);

        return response.Data;
    }

    public async Task<IReadOnlyList<ApplicationVersionItem>?> GetVersionsAsync(
        Guid applicationPublicId,
        CancellationToken cancellationToken)
    {
        var response = await SendAsync<
            IReadOnlyList<ApplicationVersionItem>>(
                HttpMethod.Get,
                $"api/applications/{applicationPublicId}/versions",
                null,
                cancellationToken);

        return response.Data;
    }

    public async Task<IReadOnlyList<InstallerProfileItem>>
        GetInstallerProfilesAsync(
            Guid applicationVersionPublicId,
            CancellationToken cancellationToken)
    {
        var response = await SendAsync<
            IReadOnlyList<InstallerProfileItem>>(
                HttpMethod.Get,
                "api/installer-profiles" +
                $"?applicationVersionPublicId={applicationVersionPublicId}",
                null,
                cancellationToken);

        return response.Data
            ?? Array.Empty<InstallerProfileItem>();
    }

    public async Task<(bool Success, string? Message)> CreateVersionAsync(
        CreateVersionViewModel model,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            model.Version,
            model.ReleaseDate,
            model.IsLatest
        };

        var response = await SendAsync<ApplicationVersionItem>(
            HttpMethod.Post,
            $"api/admin/applications/{model.ApplicationPublicId}/versions",
            payload,
            cancellationToken,
            throwOnFailure: false);

        return (response.Success, response.Message);
    }

    public async Task<(bool Success, string? Message)>
        CreateInstallerProfileAsync(
            CreateInstallerProfileViewModel model,
            CancellationToken cancellationToken)
    {
        var payload = new
        {
            model.ApplicationVersionPublicId,
            model.InstallerTypePublicId,
            model.ArchitecturePublicId,
            model.DownloadUrl,
            model.Sha256,
            model.FileSizeBytes,
            model.SilentInstallArguments,
            model.SilentUninstallArguments,
            model.RequiresAdministrator,
            model.IsPortable,
            model.IsEnabled
        };

        var response = await SendAsync<InstallerProfileItem>(
            HttpMethod.Post,
            "api/admin/installer-profiles",
            payload,
            cancellationToken,
            throwOnFailure: false);

        return (response.Success, response.Message);
    }

    public async Task<(bool Success, string? Message)>
        DeactivateInstallerProfileAsync(
            Guid profilePublicId,
            CancellationToken cancellationToken)
    {
        using var response = await httpClient.DeleteAsync(
            $"api/admin/installer-profiles/{profilePublicId}",
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        var apiResponse = TryDeserialize<InstallerProfileItem>(body);

        return (
            false,
            apiResponse?.Message ??
            $"The API returned {(int)response.StatusCode}.");
    }

    private async Task<ApiResponse<T>> SendAsync<T>(
        HttpMethod method,
        string url,
        object? payload,
        CancellationToken cancellationToken,
        bool throwOnFailure = true)
    {
        using var request = new HttpRequestMessage(method, url);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        var result = TryDeserialize<T>(body);

        if (result is null)
        {
            throw new InvalidOperationException(
                $"SmartInstaller API returned an invalid response. " +
                $"HTTP {(int)response.StatusCode}.{Environment.NewLine}{body}");
        }

        if (throwOnFailure && !response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                result.Message ??
                $"SmartInstaller API returned {(int)response.StatusCode}.");
        }

        return result;
    }

    private static ApiResponse<T>? TryDeserialize<T>(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ApiResponse<T>>(
                body,
                JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
