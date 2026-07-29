using System.Net;
using System.Net.Http.Json;
using SmartInstaller.Services.Agent.DTOs;
using SmartInstaller.Services.Applications.DTOs;
using SmartInstaller.Services.Common.Models;
using SmartInstaller.Services.InstallerProfiles.DTOs;

namespace SmartInstaller.Tests.Integration;

[Collection(ApiTestCollection.Name)]
public sealed class AgentApiTests
{
    private const string ApplicationId =
        "70000000-0000-0000-0000-000000000005";

    private const string ExeInstallerTypeId =
        "30000000-0000-0000-0000-000000000001";

    private const string X64ArchitectureId =
        "20000000-0000-0000-0000-000000000002";

    private readonly HttpClient _client;

    public AgentApiTests(SmartInstallerApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCatalog_ReturnsLatestVersionWithCompatibleInstaller()
    {
        var setup = await CreateLatestVersionWithInstallerAsync("210.0.0");

        var response = await _client.GetAsync(
            "/api/agent/catalog?architecture=x64");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<AgentCatalogItemDto>>>();

        Assert.NotNull(result?.Data);

        var application = Assert.Single(
            result.Data,
            item => item.ApplicationId == Guid.Parse(ApplicationId));

        Assert.Equal("210.0.0", application.LatestVersion);
        Assert.Contains(
            application.Installers,
            installer => installer.InstallerProfileId == setup.Profile.PublicId);
    }

    [Fact]
    public async Task GetCatalog_WithIncompatibleArchitecture_HidesInstaller()
    {
        await CreateLatestVersionWithInstallerAsync("211.0.0");

        var response = await _client.GetAsync(
            "/api/agent/catalog?architecture=ARM64");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<AgentCatalogItemDto>>>();

        Assert.NotNull(result?.Data);
        Assert.DoesNotContain(
            result.Data,
            item => item.ApplicationId == Guid.Parse(ApplicationId));
    }

    [Fact]
    public async Task CheckUpdates_WithOlderVersion_ReturnsAvailableUpdate()
    {
        var setup = await CreateLatestVersionWithInstallerAsync("220.0.0");

        var request = new CheckUpdatesRequest(
            "x64",
            [new InstalledApplicationRequest(
                Guid.Parse(ApplicationId),
                "219.0.0")]);

        var response = await _client.PostAsJsonAsync(
            "/api/agent/check-updates",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<UpdateCheckItemDto>>>();

        Assert.NotNull(result?.Data);
        var update = Assert.Single(result.Data);
        Assert.True(update.UpdateAvailable);
        Assert.Equal("220.0.0", update.LatestVersion);
        Assert.Equal(setup.Profile.PublicId, update.InstallerProfileId);
    }

    [Fact]
    public async Task CheckUpdates_WithCurrentVersion_ReturnsNoUpdate()
    {
        await CreateLatestVersionWithInstallerAsync("230.0.0");

        var request = new CheckUpdatesRequest(
            "x64",
            [new InstalledApplicationRequest(
                Guid.Parse(ApplicationId),
                "230.0.0")]);

        var response = await _client.PostAsJsonAsync(
            "/api/agent/check-updates",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<UpdateCheckItemDto>>>();

        Assert.NotNull(result?.Data);
        var update = Assert.Single(result.Data);
        Assert.False(update.UpdateAvailable);
        Assert.Null(update.InstallerProfileId);
    }

    [Fact]
    public async Task CheckUpdates_WithoutArchitecture_ReturnsBadRequest()
    {
        var request = new CheckUpdatesRequest(
            "",
            []);

        var response = await _client.PostAsJsonAsync(
            "/api/agent/check-updates",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetInstallerManifest_WithExistingProfile_ReturnsManifest()
    {
        var setup = await CreateLatestVersionWithInstallerAsync("240.0.0");

        var response = await _client.GetAsync(
            $"/api/agent/installer-manifest/{setup.Profile.PublicId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ApiResponse<InstallerManifestDto>>();

        Assert.NotNull(result?.Data);
        Assert.Equal(setup.Profile.PublicId, result.Data.InstallerProfileId);
        Assert.Equal(Guid.Parse(ApplicationId), result.Data.ApplicationId);
        Assert.Equal("240.0.0", result.Data.Version);
        Assert.Equal("EXE", result.Data.InstallerType);
        Assert.Equal("x64", result.Data.Architecture);
        Assert.Equal("/S", result.Data.SilentInstallArguments);
    }

    [Fact]
    public async Task GetInstallerManifest_WithUnknownProfile_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            $"/api/agent/installer-manifest/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<(ApplicationVersionDto Version, InstallerProfileDto Profile)>
        CreateLatestVersionWithInstallerAsync(string versionName)
    {
        var versionResponse = await _client.PostAsJsonAsync(
            $"/api/admin/applications/{ApplicationId}/versions",
            new CreateApplicationVersionRequest(
                versionName,
                DateTime.UtcNow,
                true));

        Assert.Equal(HttpStatusCode.Created, versionResponse.StatusCode);

        var versionResult = await versionResponse.Content
            .ReadFromJsonAsync<ApiResponse<ApplicationVersionDto>>();

        Assert.NotNull(versionResult?.Data);

        var profileResponse = await _client.PostAsJsonAsync(
            "/api/admin/installer-profiles",
            new CreateInstallerProfileRequest(
                versionResult.Data.PublicId,
                Guid.Parse(ExeInstallerTypeId),
                Guid.Parse(X64ArchitectureId),
                $"https://downloads.example.test/{versionName}/setup.exe",
                new string('a', 64),
                12_345_678,
                "/S",
                "/uninstall /S",
                true,
                false,
                true));

        Assert.Equal(HttpStatusCode.Created, profileResponse.StatusCode);

        var profileResult = await profileResponse.Content
            .ReadFromJsonAsync<ApiResponse<InstallerProfileDto>>();

        Assert.NotNull(profileResult?.Data);
        return (versionResult.Data, profileResult.Data);
    }
}
