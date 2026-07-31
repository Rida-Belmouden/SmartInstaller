namespace SmartInstaller.Agent.Core.Configuration;

public sealed class AgentOptions
{
    public const string SectionName = "Agent";

    public string ApiBaseUrl { get; set; } = "http://localhost:5272";

    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
