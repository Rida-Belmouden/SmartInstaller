namespace SmartInstaller.Agent.Core.Installation.Models;

public enum InstallStatus
{
    Pending,
    Running,
    Succeeded,
    Failed,
    Cancelled,
    TimedOut,
    RestartRequired,
    FileNotFound,
    UnsupportedInstaller
}
