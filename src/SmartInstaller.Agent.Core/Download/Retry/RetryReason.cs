namespace SmartInstaller.Agent.Core.Download.Retry;

public enum RetryReason
{
    None,
    RequestTimeout,
    TooManyRequests,
    ServerError,
    NetworkFailure,
    RetryAfterHeader,
    MaxAttemptsReached,
    PermanentFailure
}
