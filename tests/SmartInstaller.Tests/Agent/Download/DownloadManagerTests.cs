using SmartInstaller.Agent.Core.Download.Cache;
using SmartInstaller.Agent.Core.Download.Http;
using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Services;
using SmartInstaller.Agent.Core.Download.Verification;
using SmartInstaller.Agent.Core.Download.Resume;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class DownloadManagerTests
{
    [Fact]
    public async Task DownloadAsync_WithSuccessfulDownload_PromotesFile()
    {
        using var cache = new TestFileCacheService();
        var downloader = new FakeHttpDownloader(
            "smart-installer");

        var manager = new DownloadManager(
            downloader,
            cache,
            new Sha256Verifier());

        var result = await manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe",
                ExpectedFileSizeBytes: 15));

        Assert.Equal(
            DownloadStatus.Completed,
            result.Status);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.FilePath);
        Assert.True(File.Exists(result.FilePath));

        Assert.Equal(
            "smart-installer",
            await File.ReadAllTextAsync(result.FilePath));

        Assert.False(File.Exists(
            cache.GetTemporaryPath("setup.exe")));
    }

    [Fact]
    public async Task DownloadAsync_WithReusableFile_ReturnsCached()
    {
        using var cache = new TestFileCacheService();
        cache.EnsureCacheDirectoryExists();

        await File.WriteAllTextAsync(
            cache.GetFinalPath("setup.exe"),
            "cached");

        var downloader = new FakeHttpDownloader(
            "new-content");

        var manager = new DownloadManager(
            downloader,
            cache,
            new Sha256Verifier());

        var result = await manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe",
                ExpectedFileSizeBytes: 6));

        Assert.Equal(
            DownloadStatus.Cached,
            result.Status);

        Assert.Equal(0, downloader.CallCount);
    }

    [Fact]
    public async Task DownloadAsync_WhenDownloaderFails_DeletesTemporaryFile()
    {
        using var cache = new TestFileCacheService();

        var downloader = FakeHttpDownloader.Failed(
            "HTTP 500");

        var manager = new DownloadManager(
            downloader,
            cache,
            new Sha256Verifier());

        var result = await manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe"));

        Assert.Equal(
            DownloadStatus.Failed,
            result.Status);

        Assert.Contains(
            "HTTP 500",
            result.ErrorMessage);

        Assert.False(File.Exists(
            cache.GetTemporaryPath("setup.exe")));
    }

    [Fact]
    public async Task DownloadAsync_WhenCancelled_DeletesTemporaryFile()
    {
        using var cache = new TestFileCacheService();

        var manager = new DownloadManager(
            FakeHttpDownloader.Cancelled(),
            cache,
            new Sha256Verifier());

        var result = await manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe"));

        Assert.Equal(
            DownloadStatus.Cancelled,
            result.Status);

        Assert.False(File.Exists(
            cache.GetTemporaryPath("setup.exe")));
    }

    [Fact]
    public async Task DownloadAsync_WithSizeMismatch_ReturnsFailed()
    {
        using var cache = new TestFileCacheService();

        var manager = new DownloadManager(
            new FakeHttpDownloader("small"),
            cache,
            new Sha256Verifier());

        var result = await manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe",
                ExpectedFileSizeBytes: 100));

        Assert.Equal(
            DownloadStatus.Failed,
            result.Status);

        Assert.Contains(
            "size",
            result.ErrorMessage);

        Assert.False(File.Exists(
            cache.GetFinalPath("setup.exe")));
    }

    [Fact]
    public async Task DownloadAsync_WithUnsupportedScheme_DoesNotCallDownloader()
    {
        using var cache = new TestFileCacheService();
        var downloader = new FakeHttpDownloader("content");

        var manager = new DownloadManager(
            downloader,
            cache,
            new Sha256Verifier());

        var result = await manager.DownloadAsync(
            new DownloadRequest(
                new Uri("ftp://example.test/setup.exe"),
                "setup.exe"));

        Assert.Equal(
            DownloadStatus.Failed,
            result.Status);

        Assert.Equal(0, downloader.CallCount);
    }

    private sealed class FakeHttpDownloader(
        string content)
        : IHttpDownloader
    {
        private readonly string? _errorMessage;
        private readonly bool _cancelled;

        private FakeHttpDownloader(
            string? errorMessage,
            bool cancelled)
            : this(string.Empty)
        {
            _errorMessage = errorMessage;
            _cancelled = cancelled;
        }

        public int CallCount { get; private set; }

        public static FakeHttpDownloader Failed(
            string errorMessage)
        {
            return new FakeHttpDownloader(
                errorMessage,
                false);
        }

        public static FakeHttpDownloader Cancelled()
        {
            return new FakeHttpDownloader(
                null,
                true);
        }

        public async Task<HttpDownloadResult> DownloadAsync(
            HttpDownloadRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            if (_cancelled)
            {
                return HttpDownloadResult.CancelledResult();
            }

            if (_errorMessage is not null)
            {
                return HttpDownloadResult.Failed(
                    _errorMessage);
            }

            Directory.CreateDirectory(
                Path.GetDirectoryName(
                    request.DestinationPath)!);

            await File.WriteAllTextAsync(
                request.DestinationPath,
                content,
                cancellationToken);

            return HttpDownloadResult.Completed(
                content.Length);
        }
    }

    private sealed class TestFileCacheService
        : IFileCacheService, IDisposable
    {
        public TestFileCacheService()
        {
            RootDirectory = Path.Combine(
                Path.GetTempPath(),
                "SmartInstaller.Tests",
                Guid.NewGuid().ToString("N"));
        }

        public string RootDirectory { get; }

        public string GetFinalPath(string fileName)
        {
            return Path.Combine(
                RootDirectory,
                fileName);
        }

        public string GetTemporaryPath(string fileName)
        {
            return GetFinalPath(fileName) +
                   ".download";
        }

        public bool IsReusable(
            string fileName,
            long? expectedFileSizeBytes)
        {
            var path = GetFinalPath(fileName);

            if (!File.Exists(path))
            {
                return false;
            }

            return !expectedFileSizeBytes.HasValue ||
                   new FileInfo(path).Length ==
                   expectedFileSizeBytes.Value;
        }

        public void EnsureCacheDirectoryExists()
        {
            Directory.CreateDirectory(
                RootDirectory);
        }

        public void DeleteTemporaryFile(string fileName)
        {
            DeleteIfExists(
                GetTemporaryPath(fileName));
        }

        public void DeleteFinalFile(string fileName)
        {
            DeleteIfExists(
                GetFinalPath(fileName));
        }

        public void PromoteTemporaryFile(
            string fileName,
            bool overwrite)
        {
            File.Move(
                GetTemporaryPath(fileName),
                GetFinalPath(fileName),
                overwrite);
        }

        public long GetTemporaryFileSize(string fileName)
        {
            return new FileInfo(
                GetTemporaryPath(fileName)).Length;
        }

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(
                    RootDirectory,
                    recursive: true);
            }
        }

        private static void DeleteIfExists(
            string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

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
    }
}
