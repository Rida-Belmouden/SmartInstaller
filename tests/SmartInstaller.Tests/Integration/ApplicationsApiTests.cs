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
}