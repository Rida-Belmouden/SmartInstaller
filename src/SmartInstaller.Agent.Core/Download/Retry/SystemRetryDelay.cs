namespace SmartInstaller.Agent.Core.Download.Retry;
public sealed class SystemRetryDelay : IRetryDelay
{
    public Task WaitAsync(TimeSpan delay, CancellationToken cancellationToken = default) =>
        delay <= TimeSpan.Zero ? Task.CompletedTask : Task.Delay(delay, cancellationToken);
}
