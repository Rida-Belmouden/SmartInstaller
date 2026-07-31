namespace SmartInstaller.Agent.Core.Configuration;

public sealed class DownloadOptions
{
    public const string SectionName = "Download";

    public string CacheDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData),
        "SmartInstaller",
        "Cache");

    public int BufferSize { get; set; } = 81_920;

    public TimeSpan RequestTimeout { get; set; } =
        TimeSpan.FromMinutes(15);
}
