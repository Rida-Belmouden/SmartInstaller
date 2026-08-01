namespace SmartInstaller.Agent.Core.Download.Retry;

public sealed record RetryDecision(bool ShouldRetry, TimeSpan Delay, RetryReason Reason)
{
    public static RetryDecision Stop(RetryReason reason) => new(false, TimeSpan.Zero, reason);
    public static RetryDecision Retry(TimeSpan delay, RetryReason reason) => new(true, delay, reason);
}
