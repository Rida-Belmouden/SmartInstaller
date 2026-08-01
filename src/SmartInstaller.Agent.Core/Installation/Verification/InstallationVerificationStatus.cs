namespace SmartInstaller.Agent.Core.Installation.Verification;

public enum InstallationVerificationStatus
{
    NotRequired,
    Verified,
    ApplicationNotFound,
    VersionMismatch,
    VersionUnavailable,
    PendingRestart
}
