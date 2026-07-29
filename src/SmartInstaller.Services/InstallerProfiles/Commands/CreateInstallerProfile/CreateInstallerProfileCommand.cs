namespace SmartInstaller.Services.InstallerProfiles
    .Commands.CreateInstallerProfile;

public sealed record CreateInstallerProfileCommand(
    Guid ApplicationVersionPublicId,
    Guid InstallerTypePublicId,
    Guid ArchitecturePublicId,
    string DownloadUrl,
    string? Sha256,
    long? FileSizeBytes,
    string? SilentInstallArguments,
    string? SilentUninstallArguments,
    bool RequiresAdministrator,
    bool IsPortable,
    bool IsEnabled);
