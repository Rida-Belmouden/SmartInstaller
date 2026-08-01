using System.Diagnostics;
using System.Net;
using SmartInstaller.Agent.Core.Download.Models;
namespace SmartInstaller.Agent.Core.Download.Http;

public sealed class HttpDownloadAttemptExecutor(HttpClient httpClient) : IHttpDownloadAttemptExecutor
{
    private const int BufferSize = 81_920;

    public async Task<HttpDownloadResult> ExecuteAsync(HttpDownloadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            using var response = await httpClient.GetAsync(request.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return HttpDownloadResult.Failed(CreateHttpErrorMessage(response.StatusCode), response.StatusCode, GetRetryAfter(response));

            var responseLength = response.Content.Headers.ContentLength;

            if (request.ExpectedFileSizeBytes.HasValue && responseLength.HasValue && request.ExpectedFileSizeBytes.Value != responseLength.Value)
                return HttpDownloadResult.Failed("The server file size does not match the expected size.");

            var totalBytes = responseLength ?? request.ExpectedFileSizeBytes;
            var stopwatch = Stopwatch.StartNew();
            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var target = new FileStream(request.DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);
            var bytesReceived = await CopyAsync(source, target, totalBytes, stopwatch, request.Progress, cancellationToken);
            await target.FlushAsync(cancellationToken);
            return HttpDownloadResult.Completed(bytesReceived);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HttpDownloadResult.CancelledResult();
        }
        catch (TaskCanceledException ex)
        {
            return HttpDownloadResult.Failed($"The download request timed out: {ex.Message}", HttpStatusCode.RequestTimeout, isTransientException: true);
        }
        catch (HttpRequestException ex)
        {
            return HttpDownloadResult.Failed($"The download request failed: {ex.Message}", isTransientException: true);
        }
        catch (IOException ex)
        {
            return HttpDownloadResult.Failed($"The downloaded file could not be saved: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return HttpDownloadResult.Failed($"Access to the download destination was denied: {ex.Message}");
        }
    }

    private static async Task<long> CopyAsync(Stream source, Stream destination, long? totalBytes, Stopwatch stopwatch, IProgress<DownloadProgress>? progress, CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];
        long bytesReceived = 0;
        while (true)
        {
            var bytesRead = await source.ReadAsync(buffer.AsMemory(), cancellationToken);
            if (bytesRead == 0) break;
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            bytesReceived += bytesRead;
            var elapsedSeconds = Math.Max(stopwatch.Elapsed.TotalSeconds, 0.001);
            double? percentage = totalBytes is > 0 ? Math.Min(100d, bytesReceived * 100d / totalBytes.Value) : null;
            progress?.Report(new DownloadProgress(bytesReceived, totalBytes, percentage, bytesReceived / elapsedSeconds));
        }
        return bytesReceived;
    }

    private static TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is { } delta) return delta;
        if (retryAfter?.Date is { } date)
        {
            var remaining = date - DateTimeOffset.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
        return null;
    }

    private static string CreateHttpErrorMessage(HttpStatusCode statusCode) =>
        $"The download server returned HTTP {(int)statusCode} ({statusCode}).";
}
