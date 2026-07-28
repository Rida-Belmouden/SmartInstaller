using System.Net;
using System.Net.Http.Json;
using SmartInstaller.Services.Applications.DTOs;
using Xunit;

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
    public async Task CreateApplicationVersion_UsingPublicRoute_ReturnsMethodNotAllowed()
    {
        const string applicationId =
            "70000000-0000-0000-0000-000000000001";

        var request = new CreateApplicationVersionRequest(
            Version: "99.0.0",
            ReleaseDate: new DateTime(2026, 7, 27),
            IsLatest: true);

        var response = await _client.PostAsJsonAsync(
            $"/api/applications/{applicationId}/versions",
            request);

        Assert.Equal(
            HttpStatusCode.MethodNotAllowed,
            response.StatusCode);
    }
}