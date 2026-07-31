using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;

namespace SmartInstaller.Agent.Core.Download.Cache;

public sealed class CachePathProvider(
    IOptions<DownloadOptions> options)
    : ICachePathProvider
{
    private readonly DownloadOptions _options = options.Value;

    public string CacheDirectory => _options.CacheDirectory;

    public string GetFinalPath(string fileName)
    {
        ValidateFileName(fileName);

        return Path.Combine(CacheDirectory, fileName);
    }

    public string GetTemporaryPath(string fileName)
    {
        return GetFinalPath(fileName) + ".download";
    }

    public void EnsureCacheDirectoryExists()
    {
        Directory.CreateDirectory(CacheDirectory);
    }

    private static void ValidateFileName(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (!string.Equals(
                fileName,
                Path.GetFileName(fileName),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The download file name must not contain a path.",
                nameof(fileName));
        }

        if (fileName.IndexOfAny(
                Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException(
                "The download file name contains invalid characters.",
                nameof(fileName));
        }
    }
}
