using System.Security.Cryptography;
using System.Text;
using SmartInstaller.Agent.Core.Download.Verification;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class Sha256VerifierTests
{
    [Fact]
    public async Task VerifyAsync_WithMatchingHash_ReturnsSuccess()
    {
        var path = await CreateFileAsync("smart-installer");

        try
        {
            var verifier = new Sha256Verifier();
            var expected = ComputeHash("smart-installer");

            var result = await verifier.VerifyAsync(
                path,
                expected.ToUpperInvariant());

            Assert.True(result.Success);
            Assert.Equal(expected, result.ActualHash);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task VerifyAsync_WithMismatchedHash_ReturnsFailure()
    {
        var path = await CreateFileAsync("smart-installer");

        try
        {
            var verifier = new Sha256Verifier();

            var result = await verifier.VerifyAsync(
                path,
                new string('a', 64));

            Assert.False(result.Success);
            Assert.NotNull(result.ActualHash);
            Assert.Contains("does not match", result.ErrorMessage);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task VerifyAsync_WithMissingExpectedHash_SkipsVerification()
    {
        var result = await new Sha256Verifier().VerifyAsync(
            "unused",
            null);

        Assert.True(result.Success);
        Assert.Null(result.ActualHash);
    }

    [Fact]
    public async Task VerifyAsync_WithMissingFile_ReturnsFailure()
    {
        var result = await new Sha256Verifier().VerifyAsync(
            Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString("N")),
            new string('a', 64));

        Assert.False(result.Success);
        Assert.Contains("does not exist", result.ErrorMessage);
    }

    [Fact]
    public async Task VerifyAsync_WithInvalidExpectedHash_ReturnsFailure()
    {
        var result = await new Sha256Verifier().VerifyAsync(
            "unused",
            "ABC");

        Assert.False(result.Success);
        Assert.Contains("64 hexadecimal", result.ErrorMessage);
    }

    private static async Task<string> CreateFileAsync(string content)
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"SmartInstaller-{Guid.NewGuid():N}.tmp");

        await File.WriteAllTextAsync(path, content);
        return path;
    }

    private static string ComputeHash(string content) =>
        Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(content)))
            .ToLowerInvariant();
}
