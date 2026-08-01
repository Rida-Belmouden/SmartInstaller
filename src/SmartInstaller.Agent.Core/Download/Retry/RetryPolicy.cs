using System.Net;
using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;
using SmartInstaller.Agent.Core.Download.Http;
namespace SmartInstaller.Agent.Core.Download.Retry;

public sealed class RetryPolicy(IOptions<RetryOptions> options) : IRetryPolicy
{
    private readonly RetryOptions _options = options.Value;

    public RetryDecision Evaluate(int completedAttempt, HttpDownloadResult result)
    {
        if (result.Success || result.Cancelled)
            return RetryDecision.Stop(RetryReason.None);

        if (completedAttempt >= Math.Max(1, _options.MaxAttempts))
            return RetryDecision.Stop(RetryReason.MaxAttemptsReached);

        var reason = GetRetryReason(result);
        if (reason == RetryReason.PermanentFailure)
            return RetryDecision.Stop(reason);

        if (_options.UseRetryAfterHeader && result.RetryAfter is { } retryAfter)
            return RetryDecision.Retry(Clamp(retryAfter), RetryReason.RetryAfterHeader);

        return RetryDecision.Retry(CalculateDelay(completedAttempt), reason);
    }

    private RetryReason GetRetryReason(HttpDownloadResult result)
    {
        if (result.IsTransientException)
            return RetryReason.NetworkFailure;

        return result.StatusCode switch
        {
            HttpStatusCode.RequestTimeout => RetryReason.RequestTimeout,
            HttpStatusCode.TooManyRequests => RetryReason.TooManyRequests,
            HttpStatusCode.InternalServerError or
            HttpStatusCode.BadGateway or
            HttpStatusCode.ServiceUnavailable or
            HttpStatusCode.GatewayTimeout => RetryReason.ServerError,
            _ => RetryReason.PermanentFailure
        };
    }

    private TimeSpan CalculateDelay(int completedAttempt)
    {
        var multiplier = _options.UseExponentialBackoff
            ? Math.Pow(2, Math.Max(0, completedAttempt - 1))
            : 1d;
        return Clamp(TimeSpan.FromMilliseconds(_options.InitialDelay.TotalMilliseconds * multiplier));
    }

    private TimeSpan Clamp(TimeSpan delay)
    {
        if (delay < TimeSpan.Zero) return TimeSpan.Zero;
        return delay <= _options.MaximumDelay ? delay : _options.MaximumDelay;
    }
}
