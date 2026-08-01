namespace SmartInstaller.Agent.Core.Models;

public sealed record InstallerManifest(
    Guid InstallerProfileId,
    Guid ApplicationId,
    string ApplicationName,
    Guid ApplicationVersionId,
    string Version,
    string InstallerType,
    string Architecture,
    string DownloadUrl,
    string? Sha256,
    long? FileSizeBytes,
    string? SilentInstallArguments,
    string? SilentUninstallArguments,
    bool RequiresAdministrator,
    bool IsPortable);
