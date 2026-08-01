namespace SmartInstaller.Agent.Core.Installation.Models;

public sealed record InstallResult(
    InstallStatus Status,
    int? ExitCode,
    TimeSpan Duration,
    string? ErrorMessage)
{
    public bool IsSuccess =>
        Status is InstallStatus.Succeeded or
        InstallStatus.RestartRequired;

    public static InstallResult Succeeded(
        int exitCode,
        TimeSpan duration) =>
        new(
            InstallStatus.Succeeded,
            exitCode,
            duration,
            null);

    public static InstallResult RestartRequired(
        int exitCode,
        TimeSpan duration) =>
        new(
            InstallStatus.RestartRequired,
            exitCode,
            duration,
            null);

    public static InstallResult Failed(
        int? exitCode,
        string errorMessage,
        TimeSpan duration = default) =>
        new(
            InstallStatus.Failed,
            exitCode,
            duration,
            errorMessage);
}
