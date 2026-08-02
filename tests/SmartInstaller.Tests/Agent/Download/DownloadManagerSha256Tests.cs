using System.Security.Cryptography;
using System.Text;
using SmartInstaller.Agent.Core.Download.Cache;
using SmartInstaller.Agent.Core.Download.Http;
using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Services;
using SmartInstaller.Agent.Core.Download.Verification;
using SmartInstaller.Agent.Core.Download.Resume;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class DownloadManagerSha256Tests
{
    [Fact]
    public async Task DownloadAsync_WithMatchingHash_Completes()
    {
        using var cache = new TestCache();
        const string content = "smart-installer";

        var manager = new DownloadManager(
            new FakeDownloader(content),
            cache,
            new Sha256Verifier());

        var result = await manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe",
                Sha256: ComputeHash(content),
                ExpectedFileSizeBytes: content.Length));

        Assert.Equal(DownloadStatus.Completed, result.Status);
        Assert.True(File.Exists(
            cache.GetFinalPath("setup.exe")));
    }

    [Fact]
    public async Task DownloadAsync_WithMismatchedHash_DeletesFiles()
    {
        using var cache = new TestCache();

        var manager = new DownloadManager(
            new FakeDownloader("smart-installer"),
            cache,
            new Sha256Verifier());

        var result = await manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe",
                Sha256: new string('a', 64)));

        Assert.Equal(
            DownloadStatus.VerificationFailed,
            result.Status);

        Assert.False(File.Exists(
            cache.GetTemporaryPath("setup.exe")));

        Assert.False(File.Exists(
            cache.GetFinalPath("setup.exe")));
    }

    [Fact]
    public async Task DownloadAsync_WithInvalidCachedHash_Redownloads()
    {
        using var cache = new TestCache();
        cache.EnsureCacheDirectoryExists();

        await File.WriteAllTextAsync(
            cache.GetFinalPath("setup.exe"),
            "invalid-cache");

        const string content = "valid-content";
        var downloader = new FakeDownloader(content);

        var manager = new DownloadManager(
            downloader,
            cache,
            new Sha256Verifier());

        var result = await manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe",
                Sha256: ComputeHash(content)));

        Assert.Equal(DownloadStatus.Completed, result.Status);
        Assert.Equal(1, downloader.CallCount);
    }

    [Fact]
    public async Task DownloadAsync_WithValidCachedHash_ReturnsCached()
    {
        using var cache = new TestCache();
        cache.EnsureCacheDirectoryExists();

        const string content = "cached-content";

        await File.WriteAllTextAsync(
            cache.GetFinalPath("setup.exe"),
            content);

        var downloader = new FakeDownloader("new-content");

        var manager = new DownloadManager(
            downloader,
            cache,
            new Sha256Verifier());

        var result = await manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe",
                Sha256: ComputeHash(content)));

        Assert.Equal(DownloadStatus.Cached, result.Status);
        Assert.Equal(0, downloader.CallCount);
    }

    private static string ComputeHash(string content) =>
        Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(content)))
            .ToLowerInvariant();

    private sealed class FakeDownloader(string content)
        : IHttpDownloader
    {
        public int CallCount { get; private set; }

        public async Task<HttpDownloadResult> DownloadAsync(
            HttpDownloadRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            Directory.CreateDirectory(
                Path.GetDirectoryName(
                    request.DestinationPath)!);

            await File.WriteAllTextAsync(
                request.DestinationPath,
                content,
                cancellationToken);

            return HttpDownloadResult.Completed(content.Length);
        }
    }

    private sealed class TestCache
        : IFileCacheService, IDisposable
    {
        private readonly string _root = Path.Combine(
            Path.GetTempPath(),
            "SmartInstaller.Tests",
            Guid.NewGuid().ToString("N"));

        public string GetFinalPath(string fileName) =>
            Path.Combine(_root, fileName);

        public string GetTemporaryPath(string fileName) =>
            GetFinalPath(fileName) + ".download";

        public bool IsReusable(
            string fileName,
            long? expectedFileSizeBytes)
        {
            var path = GetFinalPath(fileName);

            if (!File.Exists(path))
                return false;

            return !expectedFileSizeBytes.HasValue ||
                   new FileInfo(path).Length ==
                   expectedFileSizeBytes.Value;
        }

        public void EnsureCacheDirectoryExists() =>
            Directory.CreateDirectory(_root);

        public void DeleteTemporaryFile(string fileName) =>
            DeleteIfExists(GetTemporaryPath(fileName));

        public void DeleteFinalFile(string fileName) =>
            DeleteIfExists(GetFinalPath(fileName));

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

        public ResumeMetadata GetResumeMetadata(string fileName)
        {
            var temporaryPath = GetTemporaryPath(fileName);
            var exists = File.Exists(temporaryPath);

            return new ResumeMetadata(
                temporaryPath,
                exists,
                exists
                    ? new FileInfo(temporaryPath).Length
                    : 0);
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, true);
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
