namespace SmartInstaller.Agent.Core.Configuration;

public sealed class ConcurrentDownloadOptions
{
    public const string SectionName = "Download:Concurrent";

    public int MaximumParallelDownloads { get; set; } = 3;
}
