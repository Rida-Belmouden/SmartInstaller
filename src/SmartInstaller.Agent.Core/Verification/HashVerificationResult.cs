namespace SmartInstaller.Agent.Core.Download.Verification;

public sealed record HashVerificationResult(
    bool Success,
    string? ActualHash,
    string? ExpectedHash,
    string? ErrorMessage)
{
    public static HashVerificationResult Skipped() =>
        new(true, null, null, null);

    public static HashVerificationResult Verified(
        string actualHash,
        string expectedHash) =>
        new(true, actualHash, expectedHash, null);

    public static HashVerificationResult Mismatch(
        string actualHash,
        string expectedHash) =>
        new(
            false,
            actualHash,
            expectedHash,
            "The downloaded file SHA-256 hash does not match the expected value.");

    public static HashVerificationResult Failed(
        string errorMessage,
        string? expectedHash = null) =>
        new(false, null, expectedHash, errorMessage);
}
