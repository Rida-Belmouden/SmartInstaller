using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartInstaller.Agent.Core;
using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Services;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class DownloadManagerTests
{
    [Fact]
    public void AddAgentCore_RegistersDownloadManager()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Agent:ApiBaseUrl"] = "http://localhost:5272/"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddAgentCore(configuration);

        using var provider = services.BuildServiceProvider();

        var manager = provider.GetRequiredService<IDownloadManager>();

        Assert.IsType<DownloadManager>(manager);
    }

    [Fact]
    public async Task DownloadAsync_InFoundationPhase_ReturnsFailedResult()
    {
        var manager = new DownloadManager();
        var request = new DownloadRequest(
            new Uri("https://example.test/setup.exe"),
            "setup.exe");

        var result = await manager.DownloadAsync(request);

        Assert.Equal(DownloadStatus.Failed, result.Status);
        Assert.False(result.IsSuccess);
        Assert.Null(result.FilePath);
        Assert.NotNull(result.ErrorMessage);
    }
}
