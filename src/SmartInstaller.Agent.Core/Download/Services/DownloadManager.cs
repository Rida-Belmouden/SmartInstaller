using System.Diagnostics;
using SmartInstaller.Agent.Core.Download.Cache;
using SmartInstaller.Agent.Core.Download.Http;
using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Verification;

namespace SmartInstaller.Agent.Core.Download.Services;

public sealed class DownloadManager(
    IHttpDownloader httpDownloader,
    IFileCacheService fileCacheService,
    ISha256Verifier sha256Verifier)
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
            return DownloadResult.Failed(validationError);

        fileCacheService.EnsureCacheDirectoryExists();

        var finalPath = fileCacheService.GetFinalPath(
            request.FileName);

        if (!request.Overwrite &&
            fileCacheService.IsReusable(
                request.FileName,
                request.ExpectedFileSizeBytes))
        {
            var cachedVerification =
                await sha256Verifier.VerifyAsync(
                    finalPath,
                    request.Sha256,
                    cancellationToken);

            if (cachedVerification.Success)
            {
                return DownloadResult.Cached(finalPath);
            }

            // The cached file is corrupted or does not match
            // the expected hash, so remove it and download again.
            fileCacheService.DeleteFinalFile(
                request.FileName);
        }

        fileCacheService.DeleteTemporaryFile(request.FileName);

        var stopwatch = Stopwatch.StartNew();

        var httpResult = await httpDownloader.DownloadAsync(
            new HttpDownloadRequest(
                request.DownloadUrl,
                fileCacheService.GetTemporaryPath(request.FileName),
                request.ExpectedFileSizeBytes,
                progress),
            cancellationToken);

        if (httpResult.Cancelled)
        {
            fileCacheService.DeleteTemporaryFile(request.FileName);
            return DownloadResult.Cancelled(stopwatch.Elapsed);
        }

        if (!httpResult.Success)
        {
            fileCacheService.DeleteTemporaryFile(request.FileName);
            return DownloadResult.Failed(
                httpResult.ErrorMessage ?? "The download failed.",
                stopwatch.Elapsed);
        }

        var downloadedLength =
            fileCacheService.GetTemporaryFileSize(
                request.FileName);

        if (request.ExpectedFileSizeBytes.HasValue &&
            downloadedLength != request.ExpectedFileSizeBytes.Value)
        {
            fileCacheService.DeleteTemporaryFile(
                request.FileName);

            return DownloadResult.Failed(
                "The downloaded file size does not match the expected size.",
                stopwatch.Elapsed);
        }

        HashVerificationResult verificationResult;

        try
        {
            verificationResult =
                await sha256Verifier.VerifyAsync(
                    fileCacheService.GetTemporaryPath(
                        request.FileName),
                    request.Sha256,
                    cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            fileCacheService.DeleteTemporaryFile(
                request.FileName);

            return DownloadResult.Cancelled(
                stopwatch.Elapsed);
        }

        if (!verificationResult.Success)
        {
            fileCacheService.DeleteTemporaryFile(
                request.FileName);

            return new DownloadResult(
                DownloadStatus.VerificationFailed,
                null,
                verificationResult.ErrorMessage ??
                "The downloaded file failed SHA-256 verification.",
                stopwatch.Elapsed);
        }

        try
        {
            fileCacheService.PromoteTemporaryFile(
                request.FileName,
                overwrite: true);

            return DownloadResult.Completed(
                finalPath,
                stopwatch.Elapsed);
        }
        catch (IOException exception)
        {
            fileCacheService.DeleteTemporaryFile(
                request.FileName);

            return DownloadResult.Failed(
                $"The downloaded file could not be finalized: {exception.Message}",
                stopwatch.Elapsed);
        }
        catch (UnauthorizedAccessException exception)
        {
            fileCacheService.DeleteTemporaryFile(
                request.FileName);

            return DownloadResult.Failed(
                $"Access to the download cache was denied: {exception.Message}",
                stopwatch.Elapsed);
        }
    }

    private static string? ValidateRequest(
        DownloadRequest request)
    {
        if (!request.DownloadUrl.IsAbsoluteUri)
            return "The download URL must be absolute.";

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
            return "The download file name is required.";

        if (request.ExpectedFileSizeBytes is < 0)
            return "The expected file size cannot be negative.";

        if (!string.IsNullOrWhiteSpace(request.Sha256))
        {
            var normalized = request.Sha256
                .Trim()
                .Replace("-", string.Empty, StringComparison.Ordinal);

            if (normalized.Length != 64 ||
                !normalized.All(Uri.IsHexDigit))
            {
                return "The SHA-256 value must contain exactly 64 hexadecimal characters.";
            }
        }

        return null;
    }
}
