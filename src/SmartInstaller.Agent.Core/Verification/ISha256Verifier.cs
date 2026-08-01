namespace SmartInstaller.Agent.Core.Download.Verification;

public interface ISha256Verifier
{
    Task<HashVerificationResult> VerifyAsync(
        string filePath,
        string? expectedSha256,
        CancellationToken cancellationToken = default);

    Task<string> ComputeHashAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}
