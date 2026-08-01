namespace SmartInstaller.Agent.Core.Installation.Processes;

public sealed record ProcessExecutionRequest(
    string FileName,
    string Arguments,
    bool RequiresAdministrator,
    bool CreateNoWindow,
    TimeSpan Timeout);
