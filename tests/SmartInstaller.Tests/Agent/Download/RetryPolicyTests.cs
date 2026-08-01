using System.Net;
using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;
using SmartInstaller.Agent.Core.Download.Http;
using SmartInstaller.Agent.Core.Download.Retry;
namespace SmartInstaller.Tests.Agent.Download;

public sealed class RetryPolicyTests
{
    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public void Evaluate_WithTransientStatus_Retries(HttpStatusCode statusCode)
    {
        var decision = CreatePolicy().Evaluate(1, HttpDownloadResult.Failed("temporary", statusCode));
        Assert.True(decision.ShouldRetry);
    }

    [Fact]
    public void Evaluate_With404_DoesNotRetry()
    {
        var decision = CreatePolicy().Evaluate(1, HttpDownloadResult.Failed("not found", HttpStatusCode.NotFound));
        Assert.False(decision.ShouldRetry);
        Assert.Equal(RetryReason.PermanentFailure, decision.Reason);
    }

    [Fact]
    public void Evaluate_AtMaximumAttempts_Stops()
    {
        var decision = CreatePolicy(3).Evaluate(3, HttpDownloadResult.Failed("temporary", HttpStatusCode.ServiceUnavailable));
        Assert.False(decision.ShouldRetry);
        Assert.Equal(RetryReason.MaxAttemptsReached, decision.Reason);
    }

    [Fact]
    public void Evaluate_UsesExponentialBackoff()
    {
        var policy = CreatePolicy(initial: TimeSpan.FromSeconds(1), maximum: TimeSpan.FromSeconds(10));
        Assert.Equal(TimeSpan.FromSeconds(1), policy.Evaluate(1, HttpDownloadResult.Failed("x", HttpStatusCode.ServiceUnavailable)).Delay);
        Assert.Equal(TimeSpan.FromSeconds(2), policy.Evaluate(2, HttpDownloadResult.Failed("x", HttpStatusCode.ServiceUnavailable)).Delay);
        Assert.Equal(TimeSpan.FromSeconds(4), policy.Evaluate(3, HttpDownloadResult.Failed("x", HttpStatusCode.ServiceUnavailable)).Delay);
    }

    [Fact]
    public void Evaluate_ClampsDelayToMaximum()
    {
        var policy = CreatePolicy(10, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15));
        var decision = policy.Evaluate(5, HttpDownloadResult.Failed("x", HttpStatusCode.ServiceUnavailable));
        Assert.Equal(TimeSpan.FromSeconds(15), decision.Delay);
    }

    [Fact]
    public void Evaluate_UsesRetryAfterHeader()
    {
        var decision = CreatePolicy(maximum: TimeSpan.FromSeconds(15)).Evaluate(
            1,
            HttpDownloadResult.Failed("busy", HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(8)));
        Assert.True(decision.ShouldRetry);
        Assert.Equal(TimeSpan.FromSeconds(8), decision.Delay);
        Assert.Equal(RetryReason.RetryAfterHeader, decision.Reason);
    }

    [Fact]
    public void Evaluate_RetriesTransientException()
    {
        var decision = CreatePolicy().Evaluate(1, HttpDownloadResult.Failed("network", isTransientException: true));
        Assert.True(decision.ShouldRetry);
        Assert.Equal(RetryReason.NetworkFailure, decision.Reason);
    }

    private static RetryPolicy CreatePolicy(int maxAttempts = 4, TimeSpan? initial = null, TimeSpan? maximum = null) =>
        new(Options.Create(new RetryOptions
        {
            MaxAttempts = maxAttempts,
            InitialDelay = initial ?? TimeSpan.FromSeconds(1),
            MaximumDelay = maximum ?? TimeSpan.FromSeconds(15),
            UseExponentialBackoff = true,
            UseRetryAfterHeader = true
        }));
}
