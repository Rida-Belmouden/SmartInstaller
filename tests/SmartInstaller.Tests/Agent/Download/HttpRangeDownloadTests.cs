using System.Net;
using System.Net.Http.Headers;
using SmartInstaller.Agent.Core.Download.Http;
using SmartInstaller.Agent.Core.Download.Models;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class HttpRangeDownloadTests
{
    [Fact]
    public async Task ExecuteAsync_WithPartialResponse_AppendsToFile()
    {
        var path = CreateTemporaryPath();

        try
        {
            await File.WriteAllTextAsync(
                path,
                "hello ");

            var handler = new RecordingHandler(
                request =>
                {
                    Assert.Equal(
                        new RangeHeaderValue(6, null),
                        request.Headers.Range);

                    var response = new HttpResponseMessage(
                        HttpStatusCode.PartialContent)
                    {
                        Content = new StringContent("world")
                    };

                    response.Content.Headers.ContentRange =
                        new ContentRangeHeaderValue(
                            6,
                            10,
                            11);

                    return response;
                });

            var executor =
                new HttpDownloadAttemptExecutor(
                    new HttpClient(handler));

            var result = await executor.ExecuteAsync(
                new HttpDownloadRequest(
                    new Uri("https://example.test/file.bin"),
                    path,
                    11,
                    null,
                    6));

            Assert.True(result.Success);
            Assert.True(result.WasResumed);
            Assert.Equal(11, result.BytesReceived);
            Assert.Equal(
                "hello world",
                await File.ReadAllTextAsync(path));
        }
        finally
        {
            DeleteDirectory(path);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenServerReturns200_RestartsFullDownload()
    {
        var path = CreateTemporaryPath();

        try
        {
            await File.WriteAllTextAsync(
                path,
                "old-partial");

            var handler = new RecordingHandler(
                request =>
                {
                    Assert.NotNull(
                        request.Headers.Range);

                    return new HttpResponseMessage(
                        HttpStatusCode.OK)
                    {
                        Content =
                            new StringContent(
                                "complete")
                    };
                });

            var executor =
                new HttpDownloadAttemptExecutor(
                    new HttpClient(handler));

            var result = await executor.ExecuteAsync(
                new HttpDownloadRequest(
                    new Uri("https://example.test/file.bin"),
                    path,
                    8,
                    null,
                    4));

            Assert.True(result.Success);
            Assert.False(result.WasResumed);
            Assert.Equal(
                "complete",
                await File.ReadAllTextAsync(path));
        }
        finally
        {
            DeleteDirectory(path);
        }
    }

    [Fact]
    public async Task ExecuteAsync_With416AndCompletePartial_ReturnsCompleted()
    {
        var path = CreateTemporaryPath();

        try
        {
            await File.WriteAllBytesAsync(
                path,
                new byte[10]);

            var handler = new RecordingHandler(
                _ => new HttpResponseMessage(
                    HttpStatusCode.RequestedRangeNotSatisfiable));

            var result =
                await new HttpDownloadAttemptExecutor(
                        new HttpClient(handler))
                    .ExecuteAsync(
                        new HttpDownloadRequest(
                            new Uri("https://example.test/file.bin"),
                            path,
                            10,
                            null,
                            10));

            Assert.True(result.Success);
            Assert.Equal(10, result.BytesReceived);
        }
        finally
        {
            DeleteDirectory(path);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ProgressStartsFromExistingBytes()
    {
        var path = CreateTemporaryPath();

        try
        {
            await File.WriteAllBytesAsync(
                path,
                new byte[5]);

            var reports =
                new List<DownloadProgress>();

            var handler = new RecordingHandler(
                _ =>
                {
                    var response = new HttpResponseMessage(
                        HttpStatusCode.PartialContent)
                    {
                        Content =
                            new ByteArrayContent(
                                new byte[5])
                    };

                    response.Content.Headers.ContentRange =
                        new ContentRangeHeaderValue(
                            5,
                            9,
                            10);

                    return response;
                });

            await new HttpDownloadAttemptExecutor(
                    new HttpClient(handler))
                .ExecuteAsync(
                    new HttpDownloadRequest(
                        new Uri("https://example.test/file.bin"),
                        path,
                        10,
                        new InlineProgress<DownloadProgress>(
                            reports.Add),
                        5));

            Assert.NotEmpty(reports);
            Assert.Equal(5, reports[0].BytesReceived);
            Assert.Equal(50d, reports[0].Percentage);
            Assert.Equal(10, reports[^1].BytesReceived);
            Assert.Equal(100d, reports[^1].Percentage);
        }
        finally
        {
            DeleteDirectory(path);
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
            "file.bin.download");
    }

    private static void DeleteDirectory(
        string filePath)
    {
        var directory =
            Path.GetDirectoryName(filePath);

        if (directory is not null &&
            Directory.Exists(directory))
        {
            Directory.Delete(
                directory,
                recursive: true);
        }
    }

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                responseFactory(request));
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
