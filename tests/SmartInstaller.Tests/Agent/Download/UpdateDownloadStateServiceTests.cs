using SmartInstaller.Agent.Core.Download.Cache;
using SmartInstaller.Agent.Core.Download.Resume;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class UpdateDownloadStateServiceTests
{
    [Fact]
    public async Task GetStateAsync_WithPartialFile_ReturnsPercentage()
    {
        using var cache = new FakeCache(
            partialBytes: 40);

        var service =
            new UpdateDownloadStateService(
                new FakeApiClient(
                    CreateManifest(100)),
                cache,
                new InstallerFileNameResolver());

        var state = await service.GetStateAsync(
            CreateUpdate());

        Assert.NotNull(state);
        Assert.True(state.HasPartialFile);
        Assert.Equal(40, state.PartialBytes);
        Assert.Equal(40d, state.Percentage);
    }

    [Fact]
    public async Task GetStateAsync_WithFinalFile_ReturnsFinalState()
    {
        using var cache = new FakeCache(
            partialBytes: 0,
            finalFile: true);

        var state =
            await new UpdateDownloadStateService(
                    new FakeApiClient(
                        CreateManifest(100)),
                    cache,
                    new InstallerFileNameResolver())
                .GetStateAsync(
                    CreateUpdate());

        Assert.NotNull(state);
        Assert.True(state.FinalFileExists);
        Assert.False(state.HasPartialFile);
    }

    private static UpdateCheckItem CreateUpdate() =>
        new(
            Guid.NewGuid(),
            "7-Zip",
            "26.01",
            "26.02",
            true,
            Guid.NewGuid());

    private static InstallerManifest CreateManifest(
        long size) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "7-Zip",
            Guid.NewGuid(),
            "26.02",
            "EXE",
            "x64",
            "https://example.test/setup.exe",
            null,   // Sha256
            size,   // FileSizeBytes
            "/S",
            null,
            true,
            false);

    private sealed class FakeApiClient(
        InstallerManifest manifest)
        : IAgentApiClient
    {
        public Task<IReadOnlyList<AgentCatalogItem>>
            GetCatalogAsync(
                string architecture,
                CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<UpdateCheckItem>>
            CheckUpdatesAsync(
                IReadOnlyCollection<InstalledApplication>
                    installedApplications,
                CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<InstallerManifest>
            GetInstallerManifestAsync(
                Guid installerProfileId,
                CancellationToken cancellationToken = default) =>
            Task.FromResult(manifest);

        public Task<IReadOnlyList<UpdateCheckItem>> CheckUpdatesAsync(
            string architecture,
            IReadOnlyList<MatchedInstalledApplication> applications,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeCache
        : IFileCacheService, IDisposable
    {
        private readonly string _root = Path.Combine(
            Path.GetTempPath(),
            "SmartInstaller.Tests",
            Guid.NewGuid().ToString("N"));

        public FakeCache(
            long partialBytes,
            bool finalFile = false)
        {
            Directory.CreateDirectory(_root);

            if (partialBytes > 0)
            {
                File.WriteAllBytes(
                    GetTemporaryPath("setup.exe"),
                    new byte[partialBytes]);
            }

            if (finalFile)
            {
                File.WriteAllBytes(
                    GetFinalPath("setup.exe"),
                    [1]);
            }
        }

        public string GetFinalPath(string fileName) =>
            Path.Combine(_root, fileName);

        public string GetTemporaryPath(string fileName) =>
            GetFinalPath(fileName) + ".download";

        public ResumeMetadata GetResumeMetadata(
            string fileName)
        {
            var path = GetTemporaryPath(fileName);
            var exists = File.Exists(path);

            return new ResumeMetadata(
                path,
                exists,
                exists
                    ? new FileInfo(path).Length
                    : 0);
        }

        public bool IsReusable(
            string fileName,
            long? expectedFileSizeBytes) =>
            File.Exists(GetFinalPath(fileName));

        public void EnsureCacheDirectoryExists() =>
            Directory.CreateDirectory(_root);

        public void DeleteTemporaryFile(string fileName) =>
            File.Delete(GetTemporaryPath(fileName));

        public void DeleteFinalFile(string fileName) =>
            File.Delete(GetFinalPath(fileName));

        public void PromoteTemporaryFile(
            string fileName,
            bool overwrite) =>
            File.Move(
                GetTemporaryPath(fileName),
                GetFinalPath(fileName),
                overwrite);

        public long GetTemporaryFileSize(string fileName) =>
            new FileInfo(
                GetTemporaryPath(fileName)).Length;

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, true);
            }
        }
    }
}
