using SmartInstaller.Agent.Core.Download.Http;
namespace SmartInstaller.Agent.Core.Download.Retry;
public interface IRetryPolicy
{
    RetryDecision Evaluate(int completedAttempt, HttpDownloadResult result);
}
