using SmartInstaller.Agent.Core.Installation.Models;

namespace SmartInstaller.Agent.Core.Models;

public sealed record UpdateInstallationResult(
    UpdateCheckItem Update,
    InstallerManifest Manifest,
    string InstallerPath,
    InstallResult InstallResult);
