namespace SmartInstaller.Services.Agent.DTOs;

public sealed record InstallerManifestDto(
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
