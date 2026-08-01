namespace SmartInstaller.Agent.Core.Configuration;

public sealed class RetryOptions
{
    public const string SectionName = "Download:Retry";
    public int MaxAttempts { get; set; } = 4;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaximumDelay { get; set; } = TimeSpan.FromSeconds(15);
    public bool UseExponentialBackoff { get; set; } = true;
    public bool UseRetryAfterHeader { get; set; } = true;
}
