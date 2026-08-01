using Microsoft.Extensions.Options;
using SmartInstaller.Agent.Core.Configuration;
using SmartInstaller.Agent.Core.Installation.Commands;
using SmartInstaller.Agent.Core.Installation.Models;
using SmartInstaller.Agent.Core.Installation.Processes;

namespace SmartInstaller.Agent.Core.Installation.Services;

public sealed class InstallerService(
    IInstallCommandBuilder commandBuilder,
    IProcessRunner processRunner,
    IOptions<InstallationOptions> options)
    : IInstallerService
{
    private readonly InstallationOptions _options =
        options.Value;

    public async Task<InstallResult> InstallAsync(
        InstallRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!File.Exists(request.InstallerPath))
        {
            return new InstallResult(
                InstallStatus.FileNotFound,
                null,
                TimeSpan.Zero,
                "The installer file does not exist.");
        }

        InstallCommand command;

        try
        {
            command = commandBuilder.Build(request);
        }
        catch (NotSupportedException exception)
        {
            return new InstallResult(
                InstallStatus.UnsupportedInstaller,
                null,
                TimeSpan.Zero,
                exception.Message);
        }
        catch (ArgumentException exception)
        {
            return InstallResult.Failed(
                null,
                exception.Message);
        }

        var execution = await processRunner.RunAsync(
            new ProcessExecutionRequest(
                command.FileName,
                command.Arguments,
                command.RequiresAdministrator,
                _options.CreateNoWindow,
                request.Timeout ??
                _options.DefaultTimeout),
            cancellationToken);

        if (execution.Cancelled)
        {
            return new InstallResult(
                InstallStatus.Cancelled,
                null,
                execution.Duration,
                execution.ErrorMessage);
        }

        if (execution.TimedOut)
        {
            return new InstallResult(
                InstallStatus.TimedOut,
                null,
                execution.Duration,
                execution.ErrorMessage);
        }

        if (!execution.Started)
        {
            return InstallResult.Failed(
                null,
                execution.ErrorMessage ??
                "The installer could not be started.",
                execution.Duration);
        }

        return MapExitCode(
            execution.ExitCode,
            execution.Duration);
    }

    private static InstallResult MapExitCode(
        int? exitCode,
        TimeSpan duration)
    {
        return exitCode switch
        {
            0 => InstallResult.Succeeded(
                0,
                duration),

            1641 or 3010 =>
                InstallResult.RestartRequired(
                    exitCode.Value,
                    duration),

            _ => InstallResult.Failed(
                exitCode,
                $"The installer exited with code " +
                $"{exitCode?.ToString() ?? "unknown"}.",
                duration)
        };
    }
}
