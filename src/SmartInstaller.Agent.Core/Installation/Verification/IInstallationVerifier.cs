namespace SmartInstaller.Agent.Core.Installation.Verification;

public interface IInstallationVerifier
{
    Task<InstallationVerificationResult> VerifyAsync(
        string applicationName,
        string expectedVersion,
        CancellationToken cancellationToken = default);
}
