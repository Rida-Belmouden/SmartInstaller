namespace SmartInstaller.Agent.Core.Installation.Verification;

public interface IInstallationVerificationDelay
{
    Task WaitAsync(
        CancellationToken cancellationToken = default);
}
