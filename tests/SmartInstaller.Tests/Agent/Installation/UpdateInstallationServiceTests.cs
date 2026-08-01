using SmartInstaller.Agent.Core.Installation.Models;
using SmartInstaller.Agent.Core.Installation.Services;
using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Tests.Agent.Installation;

public sealed class UpdateInstallationServiceTests
{
    [Theory]
    [InlineData("EXE", InstallerKind.Exe)]
    [InlineData("exe", InstallerKind.Exe)]
    [InlineData("MSI", InstallerKind.Msi)]
    public async Task InstallAsync_MapsSupportedInstallerType(
        string installerType,
        InstallerKind expectedKind)
    {
        var installer = new RecordingInstallerService(
            InstallResult.Succeeded(
                0,
                TimeSpan.FromSeconds(1)));

        var service =
            new UpdateInstallationService(installer);

        var result = await service.InstallAsync(
            CreateUpdate(),
            CreateManifest(installerType),
            @"C:\Cache\setup.bin");

        Assert.Equal(expectedKind, installer.LastRequest!.InstallerKind);
        Assert.Equal("/S", installer.LastRequest.SilentArguments);
        Assert.True(installer.LastRequest.RequiresAdministrator);
        Assert.True(result.InstallResult.IsSuccess);
    }

    [Fact]
    public async Task InstallAsync_WithUnsupportedType_DoesNotCallInstaller()
    {
        var installer = new RecordingInstallerService(
            InstallResult.Succeeded(
                0,
                TimeSpan.Zero));

        var result =
            await new UpdateInstallationService(installer)
                .InstallAsync(
                    CreateUpdate(),
                    CreateManifest("MSIX"),
                    @"C:\Cache\setup.msix");

        Assert.Equal(
            InstallStatus.UnsupportedInstaller,
            result.InstallResult.Status);

        Assert.Null(installer.LastRequest);
    }

    [Fact]
    public async Task InstallAsync_WithPortablePackage_DoesNotCallInstaller()
    {
        var installer = new RecordingInstallerService(
            InstallResult.Succeeded(
                0,
                TimeSpan.Zero));

        var manifest = CreateManifest("EXE") with
        {
            IsPortable = true
        };

        var result =
            await new UpdateInstallationService(installer)
                .InstallAsync(
                    CreateUpdate(),
                    manifest,
                    @"C:\Cache\portable.exe");

        Assert.Equal(
            InstallStatus.UnsupportedInstaller,
            result.InstallResult.Status);

        Assert.Null(installer.LastRequest);
    }

    [Fact]
    public async Task InstallAsync_PreservesInstallerResult()
    {
        var expected = InstallResult.RestartRequired(
            3010,
            TimeSpan.FromSeconds(4));

        var result =
            await new UpdateInstallationService(
                    new RecordingInstallerService(expected))
                .InstallAsync(
                    CreateUpdate(),
                    CreateManifest("EXE"),
                    @"C:\Cache\setup.exe");

        Assert.Equal(expected, result.InstallResult);
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

    private sealed class RecordingInstallerService(
        InstallResult result)
        : IInstallerService
    {
        public InstallRequest? LastRequest { get; private set; }

        public Task<InstallResult> InstallAsync(
            InstallRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(result);
        }
    }
}
