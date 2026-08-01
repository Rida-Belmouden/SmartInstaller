namespace SmartInstaller.Agent.Core.Installation.Verification;

public sealed class InstallationVerificationDelay
    : IInstallationVerificationDelay
{
    private static readonly TimeSpan Delay =
        TimeSpan.FromSeconds(2);

    public Task WaitAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.Delay(
            Delay,
            cancellationToken);
    }
}
