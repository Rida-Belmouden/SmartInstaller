using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;
using SmartInstaller.Agent.Core.Installation.Commands;
using SmartInstaller.Agent.Core.Installation.Models;
using SmartInstaller.Agent.Core.Installation.Processes;
using SmartInstaller.Agent.Core.Installation.Services;

namespace SmartInstaller.Tests.Agent.Installation;

public sealed class InstallerServiceTests
{
    [Fact]
    public async Task InstallAsync_WhenFileMissing_ReturnsFileNotFound()
    {
        var service = CreateService(
            new FakeProcessRunner(
                CreateExecution(exitCode: 0)));

        var result = await service.InstallAsync(
            new InstallRequest(
                Path.Combine(
                    Path.GetTempPath(),
                    Guid.NewGuid() + ".exe"),
                InstallerKind.Exe,
                "/S"));

        Assert.Equal(
            InstallStatus.FileNotFound,
            result.Status);
    }

    [Theory]
    [InlineData(0, InstallStatus.Succeeded)]
    [InlineData(1641, InstallStatus.RestartRequired)]
    [InlineData(3010, InstallStatus.RestartRequired)]
    [InlineData(1, InstallStatus.Failed)]
    public async Task InstallAsync_MapsExitCode(
        int exitCode,
        InstallStatus expectedStatus)
    {
        var path = await CreateInstallerFileAsync();

        try
        {
            var service = CreateService(
                new FakeProcessRunner(
                    CreateExecution(exitCode)));

            var result = await service.InstallAsync(
                new InstallRequest(
                    path,
                    InstallerKind.Exe,
                    "/S"));

            Assert.Equal(
                expectedStatus,
                result.Status);

            Assert.Equal(
                exitCode,
                result.ExitCode);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task InstallAsync_WhenCancelled_ReturnsCancelled()
    {
        var path = await CreateInstallerFileAsync();

        try
        {
            var execution = new ProcessExecutionResult(
                true,
                true,
                false,
                null,
                TimeSpan.FromSeconds(1),
                "cancelled");

            var result = await CreateService(
                    new FakeProcessRunner(execution))
                .InstallAsync(
                    new InstallRequest(
                        path,
                        InstallerKind.Exe));

            Assert.Equal(
                InstallStatus.Cancelled,
                result.Status);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task InstallAsync_WhenTimedOut_ReturnsTimedOut()
    {
        var path = await CreateInstallerFileAsync();

        try
        {
            var execution = new ProcessExecutionResult(
                true,
                false,
                true,
                null,
                TimeSpan.FromMinutes(30),
                "timeout");

            var result = await CreateService(
                    new FakeProcessRunner(execution))
                .InstallAsync(
                    new InstallRequest(
                        path,
                        InstallerKind.Exe));

            Assert.Equal(
                InstallStatus.TimedOut,
                result.Status);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static InstallerService CreateService(
        IProcessRunner processRunner)
    {
        return new InstallerService(
            new InstallCommandBuilder(),
            processRunner,
            Options.Create(
                new InstallationOptions
                {
                    DefaultTimeout =
                        TimeSpan.FromMinutes(30),
                    CreateNoWindow = true
                }));
    }

    private static ProcessExecutionResult CreateExecution(
        int exitCode)
    {
        return new ProcessExecutionResult(
            true,
            false,
            false,
            exitCode,
            TimeSpan.FromSeconds(2),
            null);
    }

    private static async Task<string>
        CreateInstallerFileAsync()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"SmartInstaller-{Guid.NewGuid():N}.exe");

        await File.WriteAllBytesAsync(
            path,
            [0]);

        return path;
    }

    private sealed class FakeProcessRunner(
        ProcessExecutionResult result)
        : IProcessRunner
    {
        public Task<ProcessExecutionResult> RunAsync(
            ProcessExecutionRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(result);
        }
    }
}
