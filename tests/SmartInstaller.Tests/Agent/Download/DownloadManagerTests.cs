using System.Net;
using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;
using SmartInstaller.Agent.Core.Download.Cache;
using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Services;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class DownloadManagerTests
{
    [Fact]
    public async Task DownloadAsync_WithSuccessfulResponse_CreatesFinalFile()
    {
        using var scope = new DownloadTestScope(
            HttpStatusCode.OK,
            "smart-installer");

        var result = await scope.Manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe",
                ExpectedFileSizeBytes: 15));

        Assert.Equal(DownloadStatus.Completed, result.Status);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.FilePath);
        Assert.True(File.Exists(result.FilePath));
        Assert.Equal(
            "smart-installer",
            await File.ReadAllTextAsync(result.FilePath));
        Assert.False(File.Exists(
            scope.PathProvider.GetTemporaryPath("setup.exe")));
    }

    [Fact]
    public async Task DownloadAsync_ReportsProgress()
    {
        using var scope = new DownloadTestScope(
            HttpStatusCode.OK,
            new string('A', 200_000));

        var reports = new List<DownloadProgress>();

        var progress = new InlineProgress<DownloadProgress>(
            reports.Add);

        var result = await scope.Manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe",
                ExpectedFileSizeBytes: 200_000),
            progress);

        Assert.Equal(DownloadStatus.Completed, result.Status);
        Assert.NotEmpty(reports);

        var finalReport = reports[^1];

        Assert.Equal(200_000, finalReport.BytesReceived);
        Assert.Equal(200_000, finalReport.TotalBytes);
        Assert.Equal(100d, finalReport.Percentage);
        Assert.True(finalReport.BytesPerSecond > 0);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task DownloadAsync_WithFailedHttpResponse_ReturnsFailed(
        HttpStatusCode statusCode)
    {
        using var scope = new DownloadTestScope(
            statusCode,
            "error");

        var result = await scope.Manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe"));

        Assert.Equal(DownloadStatus.Failed, result.Status);
        Assert.False(result.IsSuccess);
        Assert.Contains(
            ((int)statusCode).ToString(),
            result.ErrorMessage);
        Assert.False(File.Exists(
            scope.PathProvider.GetTemporaryPath("setup.exe")));
    }

    [Fact]
    public async Task DownloadAsync_WithExistingMatchingFile_ReturnsCached()
    {
        using var scope = new DownloadTestScope(
            HttpStatusCode.OK,
            "new-content");

        scope.PathProvider.EnsureCacheDirectoryExists();

        var path = scope.PathProvider.GetFinalPath(
            "setup.exe");

        await File.WriteAllTextAsync(
            path,
            "cached");

        var result = await scope.Manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe",
                ExpectedFileSizeBytes: 6));

        Assert.Equal(DownloadStatus.Cached, result.Status);
        Assert.Equal("cached", await File.ReadAllTextAsync(path));
        Assert.Equal(0, scope.Handler.RequestCount);
    }

    [Fact]
    public async Task DownloadAsync_WithCancellation_RemovesPartialFile()
    {
        using var scope = new DownloadTestScope(
            new SlowHttpMessageHandler());

        using var cancellation =
            new CancellationTokenSource(
                TimeSpan.FromMilliseconds(50));

        var result = await scope.Manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe"),
            cancellationToken: cancellation.Token);

        Assert.Equal(DownloadStatus.Cancelled, result.Status);
        Assert.False(File.Exists(
            scope.PathProvider.GetTemporaryPath("setup.exe")));
        Assert.False(File.Exists(
            scope.PathProvider.GetFinalPath("setup.exe")));
    }

    [Fact]
    public async Task DownloadAsync_WithSizeMismatch_ReturnsFailed()
    {
        using var scope = new DownloadTestScope(
            HttpStatusCode.OK,
            "small");

        var result = await scope.Manager.DownloadAsync(
            new DownloadRequest(
                new Uri("https://example.test/setup.exe"),
                "setup.exe",
                ExpectedFileSizeBytes: 100));

        Assert.Equal(DownloadStatus.Failed, result.Status);
        Assert.Contains("size", result.ErrorMessage);
        Assert.False(File.Exists(
            scope.PathProvider.GetFinalPath("setup.exe")));
    }

    [Fact]
    public async Task DownloadAsync_WithUnsupportedScheme_ReturnsFailed()
    {
        using var scope = new DownloadTestScope(
            HttpStatusCode.OK,
            "content");

        var result = await scope.Manager.DownloadAsync(
            new DownloadRequest(
                new Uri("ftp://example.test/setup.exe"),
                "setup.exe"));

        Assert.Equal(DownloadStatus.Failed, result.Status);
        Assert.Contains("HTTP", result.ErrorMessage);
        Assert.Equal(0, scope.Handler.RequestCount);
    }

    private sealed class DownloadTestScope : IDisposable
    {
        public DownloadTestScope(
            HttpStatusCode statusCode,
            string content)
            : this(new TestHttpMessageHandler(
                statusCode,
                content))
        {
        }

        public DownloadTestScope(
            HttpMessageHandler handler)
        {
            CacheDirectory = Path.Combine(
                Path.GetTempPath(),
                "SmartInstaller.Tests",
                Guid.NewGuid().ToString("N"));

            Handler = handler as TestHttpMessageHandler
                ?? new TestHttpMessageHandlerProxy(handler);

            PathProvider = new CachePathProvider(
                Options.Create(
                    new DownloadOptions
                    {
                        CacheDirectory = CacheDirectory
                    }));

            Manager = new DownloadManager(
                new HttpClient(handler),
                PathProvider);
        }

        public string CacheDirectory { get; }

        public TestHttpMessageHandler Handler { get; }

        public CachePathProvider PathProvider { get; }

        public DownloadManager Manager { get; }

        public void Dispose()
        {
            if (Directory.Exists(CacheDirectory))
            {
                Directory.Delete(
                    CacheDirectory,
                    recursive: true);
            }
        }
    }

    private class TestHttpMessageHandler(
        HttpStatusCode statusCode,
        string content)
        : HttpMessageHandler
    {
        public int RequestCount { get; protected set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;

            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            };

            return Task.FromResult(response);
        }
    }

    private sealed class TestHttpMessageHandlerProxy(
        HttpMessageHandler innerHandler)
        : TestHttpMessageHandler(
            HttpStatusCode.OK,
            string.Empty)
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;

            return new HttpMessageInvoker(
                innerHandler)
                .SendAsync(
                    request,
                    cancellationToken);
        }
    }

    private sealed class SlowHttpMessageHandler
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(
                HttpStatusCode.OK)
            {
                Content = new StreamContent(
                    new SlowStream())
            };

            return Task.FromResult(response);
        }
    }

    private sealed class SlowStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(
            byte[] buffer,
            int offset,
            int count)
        {
            throw new NotSupportedException();
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(
                TimeSpan.FromSeconds(10),
                cancellationToken);

            return 0;
        }

        public override void Flush()
        {
        }

        public override long Seek(
            long offset,
            SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(
            byte[] buffer,
            int offset,
            int count)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class InlineProgress<T>(
        Action<T> callback)
        : IProgress<T>
    {
        public void Report(T value)
        {
            callback(value);
        }
    }
}
