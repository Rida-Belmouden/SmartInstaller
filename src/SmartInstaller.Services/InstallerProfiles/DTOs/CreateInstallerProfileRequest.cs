namespace SmartInstaller.Services.InstallerProfiles.DTOs;

public sealed record CreateInstallerProfileRequest(
    Guid ApplicationVersionPublicId,
    Guid InstallerTypePublicId,
    Guid ArchitecturePublicId,
    string DownloadUrl,
    string? Sha256,
    long? FileSizeBytes,
    string? SilentInstallArguments,
    string? SilentUninstallArguments,
    bool RequiresAdministrator = true,
    bool IsPortable = false,
    bool IsEnabled = true);
