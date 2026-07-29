using System.Net;
using System.Net.Http.Json;
using SmartInstaller.Services.Applications.DTOs;
using SmartInstaller.Services.Common.Models;
using SmartInstaller.Services.InstallerProfiles.DTOs;

namespace SmartInstaller.Tests.Integration;

[Collection(ApiTestCollection.Name)]
public sealed class InstallerProfilesApiTests
{
    private const string ApplicationId =
        "70000000-0000-0000-0000-000000000005";

    private const string ExeInstallerTypeId =
        "30000000-0000-0000-0000-000000000001";

    private const string MsiInstallerTypeId =
        "30000000-0000-0000-0000-000000000002";

    private const string X64ArchitectureId =
        "20000000-0000-0000-0000-000000000002";

    private const string Arm64ArchitectureId =
        "20000000-0000-0000-0000-000000000003";

    private readonly HttpClient _client;

    public InstallerProfilesApiTests(
        SmartInstallerApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateInstallerProfile_WithValidData_ReturnsCreated()
    {
        var versionId = await CreateApplicationVersionAsync();
        var request = CreateRequest(versionId);

        var response = await _client.PostAsJsonAsync(
            "/api/admin/installer-profiles",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ApiResponse<InstallerProfileDto>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(versionId, result.Data.ApplicationVersionPublicId);
        Assert.Equal("EXE", result.Data.InstallerTypeName);
        Assert.Equal("x64", result.Data.ArchitectureName);
        Assert.True(result.Data.IsActive);
    }

    [Fact]
    public async Task GetInstallerProfile_WithExistingId_ReturnsProfile()
    {
        var created = await CreateInstallerProfileAsync();

        var response = await _client.GetAsync(
            $"/api/installer-profiles/{created.PublicId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ApiResponse<InstallerProfileDto>>();

        Assert.NotNull(result?.Data);
        Assert.Equal(created.PublicId, result.Data.PublicId);
        Assert.Equal(created.DownloadUrl, result.Data.DownloadUrl);
    }

    [Fact]
    public async Task GetInstallerProfile_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            $"/api/installer-profiles/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ApiResponse<InstallerProfileDto>>();

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal("Installer profile was not found.", result.Message);
    }

    [Fact]
    public async Task GetInstallerProfiles_WithVersionFilter_ReturnsMatchingProfile()
    {
        var created = await CreateInstallerProfileAsync();

        var response = await _client.GetAsync(
            $"/api/installer-profiles?applicationVersionId={created.ApplicationVersionPublicId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<InstallerProfileDto>>>();

        Assert.NotNull(result?.Data);
        var profile = Assert.Single(result.Data);
        Assert.Equal(created.PublicId, profile.PublicId);
    }

    [Fact]
    public async Task CreateInstallerProfile_WithDuplicateCombination_ReturnsConflict()
    {
        var versionId = await CreateApplicationVersionAsync();
        var request = CreateRequest(versionId);

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/admin/installer-profiles",
            request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var duplicateResponse = await _client.PostAsJsonAsync(
            "/api/admin/installer-profiles",
            request);

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task CreateInstallerProfile_WithUnknownVersion_ReturnsNotFound()
    {
        var request = CreateRequest(Guid.NewGuid());

        var response = await _client.PostAsJsonAsync(
            "/api/admin/installer-profiles",
            request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.test/setup.exe")]
    public async Task CreateInstallerProfile_WithInvalidUrl_ReturnsBadRequest(
        string downloadUrl)
    {
        var versionId = await CreateApplicationVersionAsync();
        var request = CreateRequest(versionId) with
        {
            DownloadUrl = downloadUrl
        };

        var response = await _client.PostAsJsonAsync(
            "/api/admin/installer-profiles",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateInstallerProfile_WithValidData_ReturnsUpdatedProfile()
    {
        var created = await CreateInstallerProfileAsync();

        var request = new UpdateInstallerProfileRequest(
            Guid.Parse(MsiInstallerTypeId),
            Guid.Parse(Arm64ArchitectureId),
            "https://downloads.example.test/setup-updated.msi",
            new string('b', 64),
            9_876_543,
            "/quiet /norestart",
            "/uninstall /quiet",
            false,
            false,
            true);

        var response = await _client.PutAsJsonAsync(
            $"/api/admin/installer-profiles/{created.PublicId}",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ApiResponse<InstallerProfileDto>>();

        Assert.NotNull(result?.Data);
        Assert.Equal("MSI", result.Data.InstallerTypeName);
        Assert.Equal("ARM64", result.Data.ArchitectureName);
        Assert.Equal(request.DownloadUrl, result.Data.DownloadUrl);
        Assert.False(result.Data.RequiresAdministrator);
    }

    [Fact]
    public async Task UpdateInstallerProfile_WithUnknownId_ReturnsNotFound()
    {
        var request = new UpdateInstallerProfileRequest(
            Guid.Parse(ExeInstallerTypeId),
            Guid.Parse(X64ArchitectureId),
            "https://downloads.example.test/setup.exe",
            null,
            null,
            "/S",
            null,
            true,
            false,
            true);

        var response = await _client.PutAsJsonAsync(
            $"/api/admin/installer-profiles/{Guid.NewGuid()}",
            request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteInstallerProfile_DeactivatesAndHidesProfile()
    {
        var created = await CreateInstallerProfileAsync();

        var deleteResponse = await _client.DeleteAsync(
            $"/api/admin/installer-profiles/{created.PublicId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/installer-profiles/{created.PublicId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteInstallerProfile_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync(
            $"/api/admin/installer-profiles/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<InstallerProfileDto> CreateInstallerProfileAsync()
    {
        var versionId = await CreateApplicationVersionAsync();

        var response = await _client.PostAsJsonAsync(
            "/api/admin/installer-profiles",
            CreateRequest(versionId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ApiResponse<InstallerProfileDto>>();

        Assert.NotNull(result?.Data);
        return result.Data;
    }

    private async Task<Guid> CreateApplicationVersionAsync()
    {
        var versionName = $"installer-profile-{Guid.NewGuid():N}";

        var response = await _client.PostAsJsonAsync(
            $"/api/admin/applications/{ApplicationId}/versions",
            new CreateApplicationVersionRequest(
                versionName,
                DateTime.UtcNow,
                false));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ApiResponse<ApplicationVersionDto>>();

        Assert.NotNull(result?.Data);
        return result.Data.PublicId;
    }

    private static CreateInstallerProfileRequest CreateRequest(
        Guid applicationVersionId)
    {
        return new CreateInstallerProfileRequest(
            applicationVersionId,
            Guid.Parse(ExeInstallerTypeId),
            Guid.Parse(X64ArchitectureId),
            "https://downloads.example.test/setup.exe",
            new string('a', 64),
            12_345_678,
            "/S",
            "/uninstall /S",
            true,
            false,
            true);
    }
}
