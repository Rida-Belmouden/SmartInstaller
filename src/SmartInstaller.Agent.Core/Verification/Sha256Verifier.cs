using System.Security.Cryptography;

namespace SmartInstaller.Agent.Core.Download.Verification;

public sealed class Sha256Verifier : ISha256Verifier
{
    public async Task<HashVerificationResult> VerifyAsync(
        string filePath,
        string? expectedSha256,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(expectedSha256))
            return HashVerificationResult.Skipped();

        var expected = Normalize(expectedSha256);

        if (!IsValid(expected))
        {
            return HashVerificationResult.Failed(
                "The expected SHA-256 value must contain exactly 64 hexadecimal characters.",
                expected);
        }

        if (!File.Exists(filePath))
        {
            return HashVerificationResult.Failed(
                "The file to verify does not exist.",
                expected);
        }

        try
        {
            var actual = await ComputeHashAsync(
                filePath,
                cancellationToken);

            return string.Equals(
                    actual,
                    expected,
                    StringComparison.OrdinalIgnoreCase)
                ? HashVerificationResult.Verified(actual, expected)
                : HashVerificationResult.Mismatch(actual, expected);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException ex)
        {
            return HashVerificationResult.Failed(
                $"The file hash could not be calculated: {ex.Message}",
                expected);
        }
        catch (UnauthorizedAccessException ex)
        {
            return HashVerificationResult.Failed(
                $"Access to the file was denied: {ex.Message}",
                expected);
        }
    }

    public async Task<string> ComputeHashAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81_920,
            useAsync: true);

        using var sha256 = SHA256.Create();

        var hash = await sha256.ComputeHashAsync(
            stream,
            cancellationToken);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Normalize(string value) =>
        value.Trim()
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

    private static bool IsValid(string value) =>
        value.Length == 64 && value.All(Uri.IsHexDigit);
}
