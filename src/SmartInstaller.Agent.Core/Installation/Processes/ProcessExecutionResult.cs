namespace SmartInstaller.Agent.Core.Installation.Processes;

public sealed record ProcessExecutionResult(
    bool Started,
    bool Cancelled,
    bool TimedOut,
    int? ExitCode,
    TimeSpan Duration,
    string? ErrorMessage);
