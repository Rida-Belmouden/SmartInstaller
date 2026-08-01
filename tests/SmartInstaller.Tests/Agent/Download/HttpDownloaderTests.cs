using System.Net;
using SmartInstaller.Agent.Core.Download.Http;
using SmartInstaller.Agent.Core.Download.Models;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class HttpDownloaderTests
{
    [Fact]
    public async Task DownloadAsync_WithSuccessfulResponse_WritesFile()
    {
        var path = CreateTemporaryPath();

        try
        {
            using var client = new HttpClient(
                new TestHttpMessageHandler(
                    HttpStatusCode.OK,
                    "download-content"));

            var downloader = new HttpDownloadAttemptExecutor(client);

            var result = await downloader.ExecuteAsync(
                new HttpDownloadRequest(
                    new Uri("https://example.test/setup.exe"),
                    path,
                    16,
                    null));

            Assert.True(result.Success);
            Assert.False(result.Cancelled);
            Assert.Equal(16, result.BytesReceived);
            Assert.Equal(
                "download-content",
                await File.ReadAllTextAsync(path));
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public async Task DownloadAsync_WithHttpFailure_ReturnsFailed()
    {
        var path = CreateTemporaryPath();

        try
        {
            using var client = new HttpClient(
                new TestHttpMessageHandler(
                    HttpStatusCode.NotFound,
                    string.Empty));

            var downloader = new HttpDownloader(client);

            var result = await downloader.DownloadAsync(
                new HttpDownloadRequest(
                    new Uri("https://example.test/setup.exe"),
                    path,
                    null,
                    null));

            Assert.False(result.Success);
            Assert.False(result.Cancelled);
            Assert.Contains("404", result.ErrorMessage);
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public async Task DownloadAsync_ReportsProgress()
    {
        var path = CreateTemporaryPath();

        try
        {
            var content = new string('A', 200_000);
            var reports = new List<DownloadProgress>();

            using var client = new HttpClient(
                new TestHttpMessageHandler(
                    HttpStatusCode.OK,
                    content));

            var downloader = new HttpDownloader(client);

            var result = await downloader.DownloadAsync(
                new HttpDownloadRequest(
                    new Uri("https://example.test/setup.exe"),
                    path,
                    content.Length,
                    new InlineProgress<DownloadProgress>(
                        reports.Add)));

            Assert.True(result.Success);
            Assert.NotEmpty(reports);
            Assert.Equal(
                100d,
                reports[^1].Percentage);
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    private static string CreateTemporaryPath()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "SmartInstaller.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);

        return Path.Combine(
            directory,
            "setup.exe.download");
    }

    private static void DeleteIfExists(string path)
    {
        var directory = Path.GetDirectoryName(path);

        if (directory is not null &&
            Directory.Exists(directory))
        {
            Directory.Delete(
                directory,
                recursive: true);
        }
    }

    private sealed class TestHttpMessageHandler(
        HttpStatusCode statusCode,
        string content)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(content)
                });
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
