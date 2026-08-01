namespace SmartInstaller.Agent.Core.Installation.Models;

public sealed record InstallRequest(
    string InstallerPath,
    InstallerKind InstallerKind,
    string? SilentArguments = null,
    bool RequiresAdministrator = false,
    TimeSpan? Timeout = null);
