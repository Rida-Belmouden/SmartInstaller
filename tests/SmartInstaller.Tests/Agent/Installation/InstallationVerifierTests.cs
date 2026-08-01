using SmartInstaller.Agent.Core.Installation.Verification;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Tests.Agent.Installation;

public sealed class InstallationVerifierTests
{
    [Fact]
    public async Task VerifyAsync_WhenVersionMatches_ReturnsVerified()
    {
        var verifier = CreateVerifier(
            CreateApplication(
                "7-Zip 26.02 (x64)",
                "26.02",
                "7 zip"));

        var result = await verifier.VerifyAsync(
            "7-Zip",
            "26.02");

        Assert.Equal(
            InstallationVerificationStatus.Verified,
            result.Status);

        Assert.True(result.IsVerified);
        Assert.Equal("26.02", result.DetectedVersion);
    }

    [Fact]
    public async Task VerifyAsync_WhenVersionHasTrailingZeros_ReturnsVerified()
    {
        var verifier = CreateVerifier(
            CreateApplication(
                "7-Zip",
                "26.2.0",
                "7 zip"));

        var result = await verifier.VerifyAsync(
            "7-Zip",
            "26.02");

        Assert.True(result.IsVerified);
    }

    [Fact]
    public async Task VerifyAsync_WhenVersionDiffers_ReturnsMismatch()
    {
        var verifier = CreateVerifier(
            CreateApplication(
                "7-Zip",
                "26.01",
                "7 zip"));

        var result = await verifier.VerifyAsync(
            "7-Zip",
            "26.02");

        Assert.Equal(
            InstallationVerificationStatus.VersionMismatch,
            result.Status);

        Assert.Equal("26.01", result.DetectedVersion);
    }

    [Fact]
    public async Task VerifyAsync_WhenApplicationMissing_ReturnsNotFound()
    {
        var verifier = CreateVerifier();

        var result = await verifier.VerifyAsync(
            "7-Zip",
            "26.02");

        Assert.Equal(
            InstallationVerificationStatus.ApplicationNotFound,
            result.Status);
    }

    [Fact]
    public async Task VerifyAsync_WhenVersionUnavailable_ReturnsVersionUnavailable()
    {
        var verifier = CreateVerifier(
            CreateApplication(
                "7-Zip",
                null,
                "7 zip"));

        var result = await verifier.VerifyAsync(
            "7-Zip",
            "26.02");

        Assert.Equal(
            InstallationVerificationStatus.VersionUnavailable,
            result.Status);
    }

    private static InstallationVerifier CreateVerifier(
        params InstalledApplication[] applications)
    {
        return new InstallationVerifier(
            new FakeScanner(applications),
            new ApplicationNameNormalizer(),
            new NoDelay());
    }

    private static InstalledApplication CreateApplication(
        string name,
        string? version,
        string normalizedName)
    {
        return new InstalledApplication(
            name,
            version,
            "Publisher",
            null,
            null,
            null,
            normalizedName,
            "registry",
            "HKLM",
            "Registry64");
    }

    private sealed class FakeScanner(
        IReadOnlyList<InstalledApplication> applications)
        : IInstalledSoftwareScanner
    {
        public Task<IReadOnlyList<InstalledApplication>> ScanAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(applications);
        }
    }

    private sealed class NoDelay
        : IInstallationVerificationDelay
    {
        public Task WaitAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
