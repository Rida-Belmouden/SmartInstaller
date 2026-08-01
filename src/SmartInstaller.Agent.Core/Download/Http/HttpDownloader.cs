using System.Diagnostics;
using System.Net;
using SmartInstaller.Agent.Core.Download.Models;

namespace SmartInstaller.Agent.Core.Download.Http;

public sealed class HttpDownloader(
    HttpClient httpClient)
    : IHttpDownloader
{
    private const int BufferSize = 81_920;

    public async Task<HttpDownloadResult> DownloadAsync(
        HttpDownloadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            using var response = await httpClient.GetAsync(
                request.DownloadUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HttpDownloadResult.Failed(
                    CreateHttpErrorMessage(response.StatusCode));
            }

            var responseLength =
                response.Content.Headers.ContentLength;

            if (request.ExpectedFileSizeBytes.HasValue &&
                responseLength.HasValue &&
                request.ExpectedFileSizeBytes.Value !=
                    responseLength.Value)
            {
                return HttpDownloadResult.Failed(
                    "The server file size does not match the expected size.");
            }

            var totalBytes =
                responseLength ??
                request.ExpectedFileSizeBytes;

            var stopwatch = Stopwatch.StartNew();

            await using (var sourceStream =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken))
            {
                await using var targetStream = new FileStream(
                    request.DestinationPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    BufferSize,
                    useAsync: true);

                var bytesReceived = await CopyAsync(
                    sourceStream,
                    targetStream,
                    totalBytes,
                    stopwatch,
                    request.Progress,
                    cancellationToken);

                await targetStream.FlushAsync(
                    cancellationToken);

                return HttpDownloadResult.Completed(
                    bytesReceived);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return HttpDownloadResult.CancelledResult();
        }
        catch (HttpRequestException exception)
        {
            return HttpDownloadResult.Failed(
                $"The download request failed: {exception.Message}");
        }
        catch (IOException exception)
        {
            return HttpDownloadResult.Failed(
                $"The downloaded file could not be saved: {exception.Message}");
        }
        catch (UnauthorizedAccessException exception)
        {
            return HttpDownloadResult.Failed(
                $"Access to the download destination was denied: {exception.Message}");
        }
    }

    private static async Task<long> CopyAsync(
        Stream source,
        Stream destination,
        long? totalBytes,
        Stopwatch stopwatch,
        IProgress<DownloadProgress>? progress,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];
        long bytesReceived = 0;

        while (true)
        {
            var bytesRead = await source.ReadAsync(
                buffer.AsMemory(),
                cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            await destination.WriteAsync(
                buffer.AsMemory(0, bytesRead),
                cancellationToken);

            bytesReceived += bytesRead;

            var elapsedSeconds = Math.Max(
                stopwatch.Elapsed.TotalSeconds,
                0.001);

            double? percentage = totalBytes is > 0
                ? Math.Min(
                    100d,
                    bytesReceived * 100d / totalBytes.Value)
                : null;

            progress?.Report(
                new DownloadProgress(
                    bytesReceived,
                    totalBytes,
                    percentage,
                    bytesReceived / elapsedSeconds));
        }

        return bytesReceived;
    }

    private static string CreateHttpErrorMessage(
        HttpStatusCode statusCode)
    {
        return
            $"The download server returned HTTP " +
            $"{(int)statusCode} ({statusCode}).";
    }
}
