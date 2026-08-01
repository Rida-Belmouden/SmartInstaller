using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Installation.Verification;

public sealed record InstallationVerificationResult(
    InstallationVerificationStatus Status,
    string ExpectedVersion,
    string? DetectedVersion,
    InstalledApplication? DetectedApplication,
    string? Message)
{
    public bool IsVerified =>
        Status == InstallationVerificationStatus.Verified;

    public static InstallationVerificationResult NotRequired(
        string expectedVersion) =>
        new(
            InstallationVerificationStatus.NotRequired,
            expectedVersion,
            null,
            null,
            null);

    public static InstallationVerificationResult PendingRestart(
        string expectedVersion) =>
        new(
            InstallationVerificationStatus.PendingRestart,
            expectedVersion,
            null,
            null,
            "Installation completed, but verification requires a restart.");
}
