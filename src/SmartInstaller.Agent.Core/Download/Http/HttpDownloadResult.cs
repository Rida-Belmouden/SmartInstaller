using System.Net;
namespace SmartInstaller.Agent.Core.Download.Http;

public sealed record HttpDownloadResult(
    bool Success,
    bool Cancelled,
    long BytesReceived,
    string? ErrorMessage,
    HttpStatusCode? StatusCode = null,
    TimeSpan? RetryAfter = null,
    bool IsTransientException = false)
{
    public static HttpDownloadResult Completed(long bytesReceived) => new(true, false, bytesReceived, null);
    public static HttpDownloadResult Failed(string errorMessage, HttpStatusCode? statusCode = null, TimeSpan? retryAfter = null, bool isTransientException = false) =>
        new(false, false, 0, errorMessage, statusCode, retryAfter, isTransientException);
    public static HttpDownloadResult CancelledResult() => new(false, true, 0, "The download was cancelled.");
}
