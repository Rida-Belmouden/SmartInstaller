using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;
using SmartInstaller.Agent.Core.Download.Cache;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class FileCacheResumeTests : IDisposable
{
    private readonly string _cacheDirectory = Path.Combine(
        Path.GetTempPath(),
        "SmartInstaller.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void GetResumeMetadata_WhenFileMissing_ReturnsZeroBytes()
    {
        var service = CreateService();

        var metadata = service.GetResumeMetadata(
            "setup.exe");

        Assert.False(metadata.Exists);
        Assert.Equal(0, metadata.ExistingBytes);
        Assert.EndsWith(
            "setup.exe.download",
            metadata.TemporaryFilePath);
    }

    [Fact]
    public async Task GetResumeMetadata_WhenFileExists_ReturnsLength()
    {
        var service = CreateService();
        service.EnsureCacheDirectoryExists();

        await File.WriteAllBytesAsync(
            service.GetTemporaryPath("setup.exe"),
            new byte[512]);

        var metadata = service.GetResumeMetadata(
            "setup.exe");

        Assert.True(metadata.Exists);
        Assert.Equal(512, metadata.ExistingBytes);
    }

    private FileCacheService CreateService()
    {
        var provider = new CachePathProvider(
            Options.Create(
                new DownloadOptions
                {
                    CacheDirectory = _cacheDirectory
                }));

        return new FileCacheService(provider);
    }

    public void Dispose()
    {
        if (Directory.Exists(_cacheDirectory))
        {
            Directory.Delete(
                _cacheDirectory,
                recursive: true);
        }
    }
}
