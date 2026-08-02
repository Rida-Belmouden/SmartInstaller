using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using SmartInstaller.Agent.Core.Download.Models;

namespace SmartInstaller.Agent.Core.Download.Http;

public sealed class HttpDownloadAttemptExecutor(
    HttpClient httpClient)
    : IHttpDownloadAttemptExecutor
{
    private const int BufferSize = 81_920;

    public async Task<HttpDownloadResult> ExecuteAsync(
        HttpDownloadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            using var message = new HttpRequestMessage(
                HttpMethod.Get,
                request.DownloadUrl);

            if (request.ResumeOffsetBytes > 0)
            {
                message.Headers.Range = new RangeHeaderValue(
                    request.ResumeOffsetBytes,
                    null);
            }

            using var response = await httpClient.SendAsync(
                message,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.StatusCode ==
                HttpStatusCode.RequestedRangeNotSatisfiable)
            {
                if (request.ExpectedFileSizeBytes.HasValue &&
                    request.ResumeOffsetBytes ==
                    request.ExpectedFileSizeBytes.Value &&
                    File.Exists(request.DestinationPath) &&
                    new FileInfo(request.DestinationPath).Length ==
                    request.ExpectedFileSizeBytes.Value)
                {
                    return HttpDownloadResult.Completed(
                        request.ResumeOffsetBytes,
                        wasResumed: true);
                }

                return HttpDownloadResult.Failed(
                    "The server rejected the requested download range.",
                    response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                return HttpDownloadResult.Failed(
                    CreateHttpErrorMessage(
                        response.StatusCode),
                    response.StatusCode,
                    GetRetryAfter(response));
            }

            var isPartialResponse =
                response.StatusCode ==
                HttpStatusCode.PartialContent;

            var appendToExisting =
                request.ResumeOffsetBytes > 0 &&
                isPartialResponse;

            if (isPartialResponse &&
                !IsValidContentRange(
                    response,
                    request.ResumeOffsetBytes))
            {
                return HttpDownloadResult.Failed(
                    "The server returned an invalid Content-Range header.");
            }

            var baseBytes = appendToExisting
                ? request.ResumeOffsetBytes
                : 0;

            var responseLength =
                response.Content.Headers.ContentLength;

            long? expectedResponseLength =
                request.ExpectedFileSizeBytes.HasValue
                    ? request.ExpectedFileSizeBytes.Value -
                      baseBytes
                    : null;

            if (expectedResponseLength is long expectedLength &&
                expectedLength >= 0 &&
                responseLength is long actualLength &&
                expectedLength != actualLength)
            {
                return HttpDownloadResult.Failed(
                    "The server file size does not match the expected size.");
            }

            var totalBytes =
                request.ExpectedFileSizeBytes ??
                GetTotalLengthFromContentRange(response) ??
                (responseLength.HasValue
                    ? baseBytes + responseLength.Value
                    : null);

            var stopwatch = Stopwatch.StartNew();

            await using var source =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken);

            await using var target = new FileStream(
                request.DestinationPath,
                appendToExisting
                    ? FileMode.Append
                    : FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                useAsync: true);

            var finalBytes = await CopyAsync(
                source,
                target,
                baseBytes,
                totalBytes,
                stopwatch,
                request.Progress,
                cancellationToken);

            await target.FlushAsync(cancellationToken);

            return HttpDownloadResult.Completed(
                finalBytes,
                appendToExisting);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return HttpDownloadResult.CancelledResult();
        }
        catch (TaskCanceledException exception)
        {
            return HttpDownloadResult.Failed(
                $"The download request timed out: {exception.Message}",
                HttpStatusCode.RequestTimeout,
                isTransientException: true);
        }
        catch (HttpRequestException exception)
        {
            return HttpDownloadResult.Failed(
                $"The download request failed: {exception.Message}",
                isTransientException: true);
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
        long baseBytes,
        long? totalBytes,
        Stopwatch stopwatch,
        IProgress<DownloadProgress>? progress,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];
        long downloadedThisAttempt = 0;

        ReportProgress(
            progress,
            baseBytes,
            totalBytes,
            0);

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

            downloadedThisAttempt += bytesRead;

            var cumulativeBytes =
                baseBytes +
                downloadedThisAttempt;

            var elapsedSeconds = Math.Max(
                stopwatch.Elapsed.TotalSeconds,
                0.001);

            ReportProgress(
                progress,
                cumulativeBytes,
                totalBytes,
                downloadedThisAttempt /
                elapsedSeconds);
        }

        return baseBytes +
               downloadedThisAttempt;
    }

    private static void ReportProgress(
        IProgress<DownloadProgress>? progress,
        long bytesReceived,
        long? totalBytes,
        double bytesPerSecond)
    {
        double? percentage = totalBytes is > 0
            ? Math.Min(
                100d,
                bytesReceived * 100d /
                totalBytes.Value)
            : null;

        progress?.Report(
            new DownloadProgress(
                bytesReceived,
                totalBytes,
                percentage,
                bytesPerSecond));
    }

    private static bool IsValidContentRange(
        HttpResponseMessage response,
        long expectedStart)
    {
        var contentRange =
            response.Content.Headers.ContentRange;

        return contentRange is not null &&
               contentRange.HasRange &&
               contentRange.From ==
               expectedStart;
    }

    private static long? GetTotalLengthFromContentRange(
        HttpResponseMessage response)
    {
        return response.Content.Headers
            .ContentRange?
            .Length;
    }

    private static TimeSpan? GetRetryAfter(
        HttpResponseMessage response)
    {
        var retryAfter =
            response.Headers.RetryAfter;

        if (retryAfter?.Delta is { } delta)
        {
            return delta;
        }

        if (retryAfter?.Date is { } date)
        {
            var remaining =
                date -
                DateTimeOffset.UtcNow;

            return remaining > TimeSpan.Zero
                ? remaining
                : TimeSpan.Zero;
        }

        return null;
    }

    private static string CreateHttpErrorMessage(
        HttpStatusCode statusCode) =>
        $"The download server returned HTTP " +
        $"{(int)statusCode} ({statusCode}).";
}
