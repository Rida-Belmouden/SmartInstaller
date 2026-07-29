namespace SmartInstaller.Services.InstallerProfiles.DTOs;

public sealed record InstallerProfileDto(
    Guid PublicId,
    Guid ApplicationVersionPublicId,
    Guid InstallerTypePublicId,
    string InstallerTypeName,
    Guid ArchitecturePublicId,
    string ArchitectureName,
    string DownloadUrl,
    string? Sha256,
    long? FileSizeBytes,
    string? SilentInstallArguments,
    string? SilentUninstallArguments,
    bool RequiresAdministrator,
    bool IsPortable,
    bool IsEnabled,
    bool IsActive);
