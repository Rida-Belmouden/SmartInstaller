namespace SmartInstaller.Agent.Core.Installation.Processes;

public interface IProcessRunner
{
    Task<ProcessExecutionResult> RunAsync(
        ProcessExecutionRequest request,
        CancellationToken cancellationToken = default);
}
