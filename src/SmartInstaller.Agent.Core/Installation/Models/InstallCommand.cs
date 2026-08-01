namespace SmartInstaller.Agent.Core.Installation.Models;

public sealed record InstallCommand(
    string FileName,
    string Arguments,
    bool RequiresAdministrator);
