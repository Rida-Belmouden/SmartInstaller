using SmartInstaller.Agent.Core.Installation.Models;

namespace SmartInstaller.Agent.Core.Installation.Commands;

public sealed class InstallCommandBuilder
    : IInstallCommandBuilder
{
    public InstallCommand Build(InstallRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(
            request.InstallerPath);

        return request.InstallerKind switch
        {
            InstallerKind.Exe => BuildExe(request),
            InstallerKind.Msi => BuildMsi(request),
            _ => throw new NotSupportedException(
                $"Installer type '{request.InstallerKind}' is not supported.")
        };
    }

    private static InstallCommand BuildExe(
        InstallRequest request)
    {
        return new InstallCommand(
            request.InstallerPath,
            request.SilentArguments?.Trim() ?? string.Empty,
            request.RequiresAdministrator);
    }

    private static InstallCommand BuildMsi(
        InstallRequest request)
    {
        var arguments =
            $"/i {Quote(request.InstallerPath)}";

        if (!string.IsNullOrWhiteSpace(
                request.SilentArguments))
        {
            arguments += " " +
                request.SilentArguments.Trim();
        }
        else
        {
            arguments += " /qn /norestart";
        }

        return new InstallCommand(
            "msiexec.exe",
            arguments,
            request.RequiresAdministrator);
    }

    private static string Quote(string value)
    {
        return $"\"{value.Replace(
            "\"",
            "\\\"",
            StringComparison.Ordinal)}\"";
    }
}
