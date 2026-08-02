using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Services;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class UpdateDownloadServiceTests
{
    [Fact]
    public async Task DownloadAsync_WithCompatibleInstaller_DownloadsManifestFile()
    {
        var update = CreateUpdate();
        var api = new FakeAgentApiClient(CreateManifest());
        var manager = new FakeDownloadManager(
            DownloadResult.Completed("C:\\cache\\setup.exe", TimeSpan.Zero));

        var service = new UpdateDownloadService(api, manager, new InstallerFileNameResolver());

        var result = await service.DownloadAsync(update);

        Assert.True(result.DownloadResult.IsSuccess);
        Assert.Equal(update.InstallerProfileId, api.RequestedInstallerProfileId);
        Assert.NotNull(manager.Request);
        Assert.Equal("setup.exe", manager.Request.FileName);
        Assert.Equal(CreateManifest().Sha256, manager.Request.Sha256);
    }

    [Fact]
    public async Task DownloadAsync_WithoutInstallerProfile_ReturnsFailure()
    {
        var update = CreateUpdate() with { InstallerProfileId = null };
        var service = new UpdateDownloadService(
            new FakeAgentApiClient(CreateManifest()),
            new FakeDownloadManager(DownloadResult.Failed("unused")),
            new InstallerFileNameResolver());

        var result = await service.DownloadAsync(update);

        Assert.Equal(DownloadStatus.Failed, result.DownloadResult.Status);
        Assert.Contains("compatible installer", result.DownloadResult.ErrorMessage);
    }

    [Fact]
    public async Task DownloadAsync_WhenNoUpdateAvailable_ReturnsFailure()
    {
        var update = CreateUpdate() with { UpdateAvailable = false };
        var service = new UpdateDownloadService(
            new FakeAgentApiClient(CreateManifest()),
            new FakeDownloadManager(DownloadResult.Failed("unused")),
            new InstallerFileNameResolver());

        var result = await service.DownloadAsync(update);

        Assert.Equal(DownloadStatus.Failed, result.DownloadResult.Status);
        Assert.Contains("does not have", result.DownloadResult.ErrorMessage);
    }

    [Fact]
    public async Task DownloadAsync_WithUrlWithoutFileName_GeneratesSafeName()
    {
        var manifest = CreateManifest() with
        {
            DownloadUrl = "https://example.test/download/",
            ApplicationName = "Example App",
            Version = "2.0",
            Architecture = "x64",
            InstallerType = "MSI"
        };

        var manager = new FakeDownloadManager(
            DownloadResult.Completed("cached", TimeSpan.Zero));

        var service = new UpdateDownloadService(
            new FakeAgentApiClient(manifest),
            manager,
            new InstallerFileNameResolver());

        await service.DownloadAsync(CreateUpdate());

        Assert.Equal("Example-App-2.0-x64.msi", manager.Request!.FileName);
    }

    private static UpdateCheckItem CreateUpdate() =>
        new(
            Guid.NewGuid(),
            "Example App",
            "1.0",
            "2.0",
            true,
            Guid.NewGuid());

    private static InstallerManifest CreateManifest() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Example App",
            Guid.NewGuid(),
            "2.0",
            "EXE",
            "x64",
            "https://example.test/files/setup.exe",
            new string('a', 64),
            100,
            "/S",
            null,
            true,
            false);

    private sealed class FakeAgentApiClient(InstallerManifest manifest)
        : IAgentApiClient
    {
        public Guid? RequestedInstallerProfileId { get; private set; }

        public Task<InstallerManifest> GetInstallerManifestAsync(
            Guid installerProfileId,
            CancellationToken cancellationToken = default)
        {
            RequestedInstallerProfileId = installerProfileId;
            return Task.FromResult(manifest);
        }

        public Task<IReadOnlyList<AgentCatalogItem>> GetCatalogAsync(
            string architecture,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<UpdateCheckItem>> CheckUpdatesAsync(
            string architecture,
            IReadOnlyList<MatchedInstalledApplication> applications,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class FakeDownloadManager(DownloadResult result)
        : IDownloadManager
    {
        public DownloadRequest? Request { get; private set; }

        public Task<DownloadResult> DownloadAsync(
            DownloadRequest request,
            IProgress<DownloadProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult(result);
        }
    }
}
