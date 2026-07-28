using System.Net;
using System.Net.Http.Json;
using SmartInstaller.Services.Applications.DTOs;
using SmartInstaller.Services.Common.Models;

namespace SmartInstaller.Tests.Integration;

[Collection(ApiTestCollection.Name)]
public sealed class ApplicationsApiTests
{
    private readonly HttpClient _client;

    public ApplicationsApiTests(
        SmartInstallerApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetApplications_ReturnsSeededApplications()
    {
        var response = await _client.GetAsync(
            "/api/applications");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<
                ApiResponse<PagedResult<ApplicationListItemDto>>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        Assert.Equal(5, result.Data.TotalItems);
        Assert.Equal(5, result.Data.Items.Count);
    }

    [Fact]
    public async Task GetApplications_WithSearch_ReturnsMatchingApplication()
    {
        var response = await _client.GetAsync(
            "/api/applications?search=visual");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<
                ApiResponse<PagedResult<ApplicationListItemDto>>>();

        Assert.NotNull(result?.Data);

        var application = Assert.Single(
            result.Data.Items);

        Assert.Equal(
            "Visual Studio Code",
            application.Name);
    }

    [Fact]
    public async Task GetApplications_WithCategory_ReturnsCategoryApplications()
    {
        var response = await _client.GetAsync(
            "/api/applications?category=browsers");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<
                ApiResponse<PagedResult<ApplicationListItemDto>>>();

        Assert.NotNull(result?.Data);
        Assert.Equal(2, result.Data.TotalItems);

        Assert.All(
            result.Data.Items,
            application =>
                Assert.Equal(
                    "Browsers",
                    application.Category));
    }

    [Fact]
    public async Task GetApplications_WithTag_ReturnsTaggedApplications()
    {
        var response = await _client.GetAsync(
            "/api/applications?tag=browser");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<
                ApiResponse<PagedResult<ApplicationListItemDto>>>();

        Assert.NotNull(result?.Data);
        Assert.Equal(2, result.Data.TotalItems);
    }

    [Fact]
    public async Task GetApplicationById_WithExistingId_ReturnsApplication()
    {
        const string publicId =
            "70000000-0000-0000-0000-000000000005";

        var response = await _client.GetAsync(
            $"/api/applications/{publicId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<
                ApiResponse<ApplicationDetailsDto>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        Assert.Equal(
            "Visual Studio Code",
            result.Data.Name);

        Assert.Equal(
            "Development",
            result.Data.Category);

        Assert.Equal(
            "Microsoft",
            result.Data.Publisher);
    }

    [Fact]
    public async Task GetApplicationById_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            $"/api/applications/{Guid.NewGuid()}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<
                ApiResponse<ApplicationDetailsDto>>();

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(
            "Application was not found.",
            result.Message);
    }

    [Fact]
    public async Task AdminCreateApplicationVersion_WithValidData_ReturnsCreated()
    {
        const string applicationId =
            "70000000-0000-0000-0000-000000000001";

        var request = new CreateApplicationVersionRequest(
            Version: "24.09",
            ReleaseDate: new DateTime(2026, 7, 27),
            IsLatest: true);

        var response = await _client.PostAsJsonAsync(
            $"/api/admin/applications/{applicationId}/versions",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<
                ApiResponse<ApplicationVersionDto>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        Assert.Equal("24.09", result.Data.Version);
        Assert.True(result.Data.IsLatest);
    }

    [Fact]
    public async Task CreateApplicationVersion_WithDuplicateVersion_ReturnsConflict()
    {
        const string applicationId =
            "70000000-0000-0000-0000-000000000002";

        var request = new CreateApplicationVersionRequest(
            Version: "3.0.21",
            ReleaseDate: new DateTime(2026, 7, 27),
            IsLatest: true);

        var firstResponse = await _client.PostAsJsonAsync(
            $"/api/admin/applications/{applicationId}/versions",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        var duplicateResponse = await _client.PostAsJsonAsync(
            $"/api/admin/applications/{applicationId}/versions",
            request);

        Assert.Equal(
            HttpStatusCode.Conflict,
            duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task CreateApplicationVersion_WithUnknownApplication_ReturnsNotFound()
    {
        var request = new CreateApplicationVersionRequest(
            Version: "1.0.0",
            ReleaseDate: DateTime.UtcNow,
            IsLatest: true);

        var response = await _client.PostAsJsonAsync(
            $"/api/admin/applications/{Guid.NewGuid()}/versions",
            request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetApplicationVersion_WithExistingId_ReturnsVersion()
    {
        const string applicationId =
            "70000000-0000-0000-0000-000000000003";

        var createRequest = new CreateApplicationVersionRequest(
            Version: "130.0",
            ReleaseDate: new DateTime(2026, 7, 27),
            IsLatest: true);

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/admin/applications/{applicationId}/versions",
            createRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var createdResult = await createResponse.Content
            .ReadFromJsonAsync<ApiResponse<ApplicationVersionDto>>();

        Assert.NotNull(createdResult);
        Assert.NotNull(createdResult.Data);

        var versionId = createdResult.Data.PublicId;

        var response = await _client.GetAsync(
            $"/api/versions/{versionId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ApiResponse<ApplicationVersionDto>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("130.0", result.Data.Version);
    }

    [Fact]
    public async Task AdminSetLatestApplicationVersion_UpdatesLatestVersion()
    {
        const string applicationId =
            "70000000-0000-0000-0000-000000000004";

        var firstResponse = await _client.PostAsJsonAsync(
            $"/api/admin/applications/{applicationId}/versions",
            new CreateApplicationVersionRequest(
                Version: "1.0.0",
                ReleaseDate: new DateTime(2026, 6, 1),
                IsLatest: true));

        var firstResult = await firstResponse.Content
            .ReadFromJsonAsync<ApiResponse<ApplicationVersionDto>>();

        Assert.NotNull(firstResult?.Data);
        Assert.True(firstResult.Data.IsLatest);

        var secondResponse = await _client.PostAsJsonAsync(
            $"/api/admin/applications/{applicationId}/versions",
            new CreateApplicationVersionRequest(
                Version: "2.0.0",
                ReleaseDate: new DateTime(2026, 7, 1),
                IsLatest: false));

        var secondResult = await secondResponse.Content
            .ReadFromJsonAsync<ApiResponse<ApplicationVersionDto>>();

        Assert.NotNull(secondResult?.Data);
        Assert.False(secondResult.Data.IsLatest);

        var setLatestResponse = await _client.PatchAsync(
            $"/api/admin/versions/{secondResult.Data.PublicId}/set-latest",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            setLatestResponse.StatusCode);

        var setLatestResult = await setLatestResponse.Content
            .ReadFromJsonAsync<ApiResponse<ApplicationVersionDto>>();

        Assert.NotNull(setLatestResult?.Data);
        Assert.True(setLatestResult.Data.IsLatest);

        var versionsResponse = await _client.GetAsync(
            $"/api/applications/{applicationId}/versions");

        Assert.Equal(
            HttpStatusCode.OK,
            versionsResponse.StatusCode);

        var versionsResult =
            await versionsResponse.Content.ReadFromJsonAsync<
                ApiResponse<IReadOnlyList<ApplicationVersionDto>>>();

        Assert.NotNull(versionsResult);
        Assert.True(versionsResult.Success);
        Assert.NotNull(versionsResult.Data);
        Assert.Equal(2, versionsResult.Data.Count);

        var firstVersion = versionsResult.Data.Single(
            version => version.Version == "1.0.0");

        var secondVersion = versionsResult.Data.Single(
            version => version.Version == "2.0.0");

        Assert.False(firstVersion.IsLatest);
        Assert.True(secondVersion.IsLatest);
    }

    [Fact]
    public async Task UpdateApplicationVersion_ReturnsOk()
    {
        const string applicationId =
            "70000000-0000-0000-0000-000000000001";

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/admin/applications/{applicationId}/versions",
            new CreateApplicationVersionRequest(
                "99.0",
                new DateTime(2026, 7, 1),
                true));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created =
            await createResponse.Content.ReadFromJsonAsync<
                ApiResponse<ApplicationVersionDto>>();

        Assert.NotNull(created?.Data);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/admin/versions/{created.Data.PublicId}",
            new UpdateApplicationVersionRequest(
                "99.1",
                new DateTime(2026, 8, 1),
                true));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated =
            await updateResponse.Content.ReadFromJsonAsync<
                ApiResponse<ApplicationVersionDto>>();

        Assert.NotNull(updated?.Data);
        Assert.Equal("99.1", updated.Data.Version);
    }

    [Fact]
    public async Task UpdateApplicationVersion_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/admin/versions/{Guid.NewGuid()}",
            new UpdateApplicationVersionRequest(
                "1.0",
                DateTime.UtcNow,
                true));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteApplicationVersion_ReturnsNoContent()
    {
        const string applicationId =
            "70000000-0000-0000-0000-000000000001";

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/admin/applications/{applicationId}/versions",
            new CreateApplicationVersionRequest(
                "100.0",
                DateTime.UtcNow,
                false));

        var created =
            await createResponse.Content.ReadFromJsonAsync<
                ApiResponse<ApplicationVersionDto>>();

        Assert.NotNull(created?.Data);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/admin/versions/{created.Data.PublicId}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteApplicationVersion_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync(
            $"/api/admin/versions/{Guid.NewGuid()}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}