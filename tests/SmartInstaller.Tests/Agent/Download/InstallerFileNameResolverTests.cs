using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class InstallerFileNameResolverTests
{
    [Fact]
    public void Resolve_WithFileNameInUrl_UsesUrlFileName()
    {
        var result =
            new InstallerFileNameResolver().Resolve(
                CreateManifest(
                    "https://example.test/files/setup.exe"),
                new Uri(
                    "https://example.test/files/setup.exe"));

        Assert.Equal("setup.exe", result);
    }

    [Fact]
    public void Resolve_WithoutFileName_CreatesSafeName()
    {
        var manifest = CreateManifest(
            "https://example.test/download");

        var result =
            new InstallerFileNameResolver().Resolve(
                manifest,
                new Uri(manifest.DownloadUrl));

        Assert.Equal(
            "7-Zip-26.02-x64.exe",
            result);
    }

    private static InstallerManifest CreateManifest(
        string url) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "7-Zip",
            Guid.NewGuid(),
            "26.02",
            "EXE",
            "x64",
            url,
            null,  // Sha256
            100,   // FileSizeBytes
            "/S",
            null,
            true,
            false);
}
