using System.Diagnostics;
using System.Net;
using SmartInstaller.Agent.Core.Download.Cache;
using SmartInstaller.Agent.Core.Download.Models;

namespace SmartInstaller.Agent.Core.Download.Services;

public sealed class DownloadManager(
    HttpClient httpClient,
    ICachePathProvider cachePathProvider)
    : IDownloadManager
{
    public async Task<DownloadResult> DownloadAsync(
        DownloadRequest request,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationError = ValidateRequest(request);

        if (validationError is not null)
        {
            return DownloadResult.Failed(validationError);
        }

        cachePathProvider.EnsureCacheDirectoryExists();

        var finalPath = cachePathProvider.GetFinalPath(
            request.FileName);

        var temporaryPath = cachePathProvider.GetTemporaryPath(
            request.FileName);

        if (!request.Overwrite &&
            IsExistingFileReusable(
                finalPath,
                request.ExpectedFileSizeBytes))
        {
            return DownloadResult.Cached(finalPath);
        }

        DeleteIfExists(temporaryPath);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await httpClient.GetAsync(
                request.DownloadUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return DownloadResult.Failed(
                    CreateHttpErrorMessage(response.StatusCode),
                    stopwatch.Elapsed);
            }

            var responseLength =
                response.Content.Headers.ContentLength;

            if (request.ExpectedFileSizeBytes.HasValue &&
                responseLength.HasValue &&
                request.ExpectedFileSizeBytes.Value != responseLength.Value)
            {
                return DownloadResult.Failed(
                    "The server file size does not match the expected size.",
                    stopwatch.Elapsed);
            }

            var totalBytes =
                responseLength ??
                request.ExpectedFileSizeBytes;

            await using (var sourceStream =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken))
            {
                await using (var targetStream = new FileStream(
                    temporaryPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81_920,
                    useAsync: true))
                {
                    await CopyToFileAsync(
                        sourceStream,
                        targetStream,
                        totalBytes,
                        stopwatch,
                        progress,
                        cancellationToken);

                    await targetStream.FlushAsync(cancellationToken);
                }
            }

            var downloadedLength =
                new FileInfo(temporaryPath).Length;

            if (request.ExpectedFileSizeBytes.HasValue &&
                downloadedLength != request.ExpectedFileSizeBytes.Value)
            {
                DeleteIfExists(temporaryPath);

                return DownloadResult.Failed(
                    "The downloaded file size does not match the expected size.",
                    stopwatch.Elapsed);
            }

            File.Move(
                temporaryPath,
                finalPath,
                overwrite: true);

            return DownloadResult.Completed(
                finalPath,
                stopwatch.Elapsed);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            DeleteIfExists(temporaryPath);

            return DownloadResult.Cancelled(
                stopwatch.Elapsed);
        }
        catch (HttpRequestException exception)
        {
            DeleteIfExists(temporaryPath);

            return DownloadResult.Failed(
                $"The download request failed: {exception.Message}",
                stopwatch.Elapsed);
        }
        catch (IOException exception)
        {
            DeleteIfExists(temporaryPath);

            return DownloadResult.Failed(
                $"The downloaded file could not be saved: {exception.Message}",
                stopwatch.Elapsed);
        }
        catch (UnauthorizedAccessException exception)
        {
            DeleteIfExists(temporaryPath);

            return DownloadResult.Failed(
                $"Access to the download cache was denied: {exception.Message}",
                stopwatch.Elapsed);
        }
    }

    private static async Task CopyToFileAsync(
        Stream source,
        Stream destination,
        long? totalBytes,
        Stopwatch stopwatch,
        IProgress<DownloadProgress>? progress,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[81_920];
        long bytesReceived = 0;

        while (true)
        {
            var bytesRead = await source.ReadAsync(
                buffer,
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
    }

    private static string? ValidateRequest(
        DownloadRequest request)
    {
        if (!request.DownloadUrl.IsAbsoluteUri)
        {
            return "The download URL must be absolute.";
        }

        if (!string.Equals(
                request.DownloadUrl.Scheme,
                Uri.UriSchemeHttp,
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                request.DownloadUrl.Scheme,
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase))
        {
            return "Only HTTP and HTTPS download URLs are supported.";
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return "The download file name is required.";
        }

        if (request.ExpectedFileSizeBytes is < 0)
        {
            return "The expected file size cannot be negative.";
        }

        return null;
    }

    private static bool IsExistingFileReusable(
        string filePath,
        long? expectedFileSizeBytes)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        return !expectedFileSizeBytes.HasValue ||
               new FileInfo(filePath).Length ==
               expectedFileSizeBytes.Value;
    }

    private static string CreateHttpErrorMessage(
        HttpStatusCode statusCode)
    {
        return
            $"The download server returned HTTP " +
            $"{(int)statusCode} ({statusCode}).";
    }

    private static void DeleteIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
