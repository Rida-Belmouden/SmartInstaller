namespace SmartInstaller.Agent.Core.Configuration;

public sealed class InstallationOptions
{
    public const string SectionName = "Installation";

    public TimeSpan DefaultTimeout { get; set; } =
        TimeSpan.FromMinutes(30);

    public bool CreateNoWindow { get; set; } = true;
}
