using SmartInstaller.Agent.Core.Installation.Models;
using SmartInstaller.Agent.Core.Installation.Verification;

namespace SmartInstaller.Agent.Core.Models;

public sealed record UpdateInstallationResult(
    UpdateCheckItem Update,
    InstallerManifest Manifest,
    string InstallerPath,
    InstallResult InstallResult,
    InstallationVerificationResult VerificationResult);
