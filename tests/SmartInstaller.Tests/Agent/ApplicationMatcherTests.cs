using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Tests.Agent;

public sealed class ApplicationMatcherTests
{
    private readonly ApplicationMatcher _matcher =
        new(new ApplicationNameNormalizer());

    [Fact]
    public void Match_WithNormalizedName_ReturnsCatalogApplication()
    {
        var installed = new[]
        {
            CreateInstalled("Google Chrome (64-bit)", "google chrome", "150.0", "Google LLC")
        };
        var catalog = new[]
        {
            CreateCatalog("Google Chrome", "Google LLC")
        };

        var match = Assert.Single(_matcher.Match(installed, catalog));

        Assert.Equal(catalog[0].ApplicationId, match.CatalogApplication.ApplicationId);
        Assert.Equal("150.0", match.InstalledApplication.Version);
    }

    [Fact]
    public void Match_WithoutInstalledVersion_IsIgnored()
    {
        var installed = new[]
        {
            CreateInstalled("Google Chrome", "google chrome", null, "Google LLC")
        };

        Assert.Empty(_matcher.Match(installed, [CreateCatalog("Google Chrome", "Google LLC")]));
    }

    [Fact]
    public void Match_DuplicateRegistryEntries_ReturnsOneApplication()
    {
        var installed = new[]
        {
            CreateInstalled("Google Chrome", "google chrome", "150.0", "Google LLC"),
            CreateInstalled("Google Chrome", "google chrome", "150.0", "Google LLC")
        };

        Assert.Single(_matcher.Match(installed, [CreateCatalog("Google Chrome", "Google LLC")]));
    }

    [Fact]
    public void Normalize_RemovesVersionAndArchitectureSuffix()
    {
        var normalizer = new ApplicationNameNormalizer();

        var installedName = normalizer.Normalize(
            "7-Zip 26.01 (x64)");

        var catalogName = normalizer.Normalize(
            "7-Zip");

        Assert.Equal(catalogName, installedName);
    }

    private static InstalledApplication CreateInstalled(
        string name, string normalizedName, string? version, string publisher) =>
        new(name, version, publisher, null, null, null, normalizedName, string.Empty, "LocalMachine", "Registry64");

    private static AgentCatalogItem CreateCatalog(string name, string publisher) =>
        new(Guid.NewGuid(), name, publisher, Guid.NewGuid(), "151.0", null, []);
}
