using SmartInstaller.Agent.Core.Installation.Models;
using SmartInstaller.Agent.Core.Installation.Services;
using SmartInstaller.Agent.Core.Installation.Verification;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Tests.Agent.Installation;

public sealed class UpdateInstallationServiceTests
{
    [Fact]
    public async Task InstallAsync_WhenInstallSucceeds_VerifiesVersion()
    {
        var verifier = new RecordingVerifier(
            new InstallationVerificationResult(
                InstallationVerificationStatus.Verified,
                "26.02",
                "26.02",
                null,
                null));

        var service = new UpdateInstallationService(
            new FakeInstallerService(
                InstallResult.Succeeded(
                    0,
                    TimeSpan.FromSeconds(1))),
            verifier);

        var result = await service.InstallAsync(
            CreateUpdate(),
            CreateManifest("EXE"),
            @"C:\Cache\setup.exe");

        Assert.True(result.InstallResult.IsSuccess);
        Assert.True(result.VerificationResult.IsVerified);
        Assert.Equal("7-Zip", verifier.ApplicationName);
        Assert.Equal("26.02", verifier.ExpectedVersion);
    }

    [Fact]
    public async Task InstallAsync_WhenRestartRequired_ReturnsPendingRestart()
    {
        var verifier = new RecordingVerifier(
            InstallationVerificationResult.NotRequired(
                "26.02"));

        var result = await new UpdateInstallationService(
                new FakeInstallerService(
                    InstallResult.RestartRequired(
                        3010,
                        TimeSpan.FromSeconds(1))),
                verifier)
            .InstallAsync(
                CreateUpdate(),
                CreateManifest("EXE"),
                @"C:\Cache\setup.exe");

        Assert.Equal(
            InstallationVerificationStatus.PendingRestart,
            result.VerificationResult.Status);

        Assert.Equal(0, verifier.CallCount);
    }

    [Fact]
    public async Task InstallAsync_WhenInstallFails_DoesNotVerify()
    {
        var verifier = new RecordingVerifier(
            InstallationVerificationResult.NotRequired(
                "26.02"));

        var result = await new UpdateInstallationService(
                new FakeInstallerService(
                    InstallResult.Failed(
                        1,
                        "failed")),
                verifier)
            .InstallAsync(
                CreateUpdate(),
                CreateManifest("EXE"),
                @"C:\Cache\setup.exe");

        Assert.False(result.InstallResult.IsSuccess);
        Assert.Equal(
            InstallationVerificationStatus.NotRequired,
            result.VerificationResult.Status);

        Assert.Equal(0, verifier.CallCount);
    }

    [Fact]
    public async Task InstallAsync_WithUnsupportedType_DoesNotInstallOrVerify()
    {
        var installer = new RecordingInstallerService();
        var verifier = new RecordingVerifier(
            InstallationVerificationResult.NotRequired(
                "26.02"));

        var result = await new UpdateInstallationService(
                installer,
                verifier)
            .InstallAsync(
                CreateUpdate(),
                CreateManifest("MSIX"),
                @"C:\Cache\setup.msix");

        Assert.Equal(
            InstallStatus.UnsupportedInstaller,
            result.InstallResult.Status);

        Assert.Equal(0, installer.CallCount);
        Assert.Equal(0, verifier.CallCount);
    }

    private static UpdateCheckItem CreateUpdate() =>
        new(
            Guid.NewGuid(),
            "7-Zip",
            "26.01",
            "26.02",
            true,
            Guid.NewGuid());

    private static InstallerManifest CreateManifest(
        string installerType) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "7-Zip",
            Guid.NewGuid(),
            "26.02",
            installerType,
            "x64",
            "https://example.test/setup.exe",
            null,
            null,
            "/S",
            null,
            true,
            false);

    private sealed class FakeInstallerService(
        InstallResult result)
        : IInstallerService
    {
        public Task<InstallResult> InstallAsync(
            InstallRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(result);
        }
    }

    private sealed class RecordingInstallerService
        : IInstallerService
    {
        public int CallCount { get; private set; }

        public Task<InstallResult> InstallAsync(
            InstallRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;

            return Task.FromResult(
                InstallResult.Succeeded(
                    0,
                    TimeSpan.Zero));
        }
    }

    private sealed class RecordingVerifier(
        InstallationVerificationResult result)
        : IInstallationVerifier
    {
        public int CallCount { get; private set; }
        public string? ApplicationName { get; private set; }
        public string? ExpectedVersion { get; private set; }

        public Task<InstallationVerificationResult> VerifyAsync(
            string applicationName,
            string expectedVersion,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            ApplicationName = applicationName;
            ExpectedVersion = expectedVersion;

            return Task.FromResult(result);
        }
    }
}
