using System.Net;
using System.Net.Http.Json;
using SmartInstaller.Services.Catalog.DTOs;
using SmartInstaller.Services.Common.Models;

namespace SmartInstaller.Tests.Integration;

[Collection(ApiTestCollection.Name)]
public sealed class CatalogApiTests
{
    private readonly HttpClient _client;

    public CatalogApiTests(
        SmartInstallerApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCategories_ReturnsSeededCategories()
    {
        var response = await _client.GetAsync(
            "/api/categories");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<
                ApiResponse<IReadOnlyCollection<CatalogItemDto>>>();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(8, result.Data.Count);

        var browsers = Assert.Single(
            result.Data,
            category => category.Slug == "browsers");

        Assert.Equal(2, browsers.ApplicationCount);
    }

    [Fact]
    public async Task GetPlatforms_ReturnsWindowsPlatform()
    {
        var response = await _client.GetAsync(
            "/api/platforms");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<
                ApiResponse<IReadOnlyCollection<CatalogItemDto>>>();

        Assert.NotNull(result?.Data);

        var platform = Assert.Single(
            result.Data);

        Assert.Equal("Windows", platform.Name);
        Assert.Equal(5, platform.ApplicationCount);
    }

    [Fact]
    public async Task GetTags_ReturnsSeededTags()
    {
        var response = await _client.GetAsync(
            "/api/tags");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<
                ApiResponse<IReadOnlyCollection<CatalogItemDto>>>();

        Assert.NotNull(result?.Data);
        Assert.Equal(7, result.Data.Count);

        var browserTag = Assert.Single(
            result.Data,
            tag => tag.Slug == "browser");

        Assert.Equal(2, browserTag.ApplicationCount);
    }
}