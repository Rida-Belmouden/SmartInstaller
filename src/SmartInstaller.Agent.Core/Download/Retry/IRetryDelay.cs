namespace SmartInstaller.Agent.Core.Download.Retry;
public interface IRetryDelay
{
    Task WaitAsync(TimeSpan delay, CancellationToken cancellationToken = default);
}
