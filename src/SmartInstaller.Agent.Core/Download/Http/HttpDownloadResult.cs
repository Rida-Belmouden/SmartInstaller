namespace SmartInstaller.Agent.Core.Download.Http;

public sealed record HttpDownloadResult(
    bool Success,
    bool Cancelled,
    long BytesReceived,
    string? ErrorMessage)
{
    public static HttpDownloadResult Completed(
        long bytesReceived)
    {
        return new HttpDownloadResult(
            true,
            false,
            bytesReceived,
            null);
    }

    public static HttpDownloadResult Failed(
        string errorMessage)
    {
        return new HttpDownloadResult(
            false,
            false,
            0,
            errorMessage);
    }

    public static HttpDownloadResult CancelledResult()
    {
        return new HttpDownloadResult(
            false,
            true,
            0,
            "The download was cancelled.");
    }
}
