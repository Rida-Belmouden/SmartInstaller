namespace SmartInstaller.Services.InstallerProfiles
    .Commands.UpdateInstallerProfile;

public sealed record UpdateInstallerProfileCommand(
    Guid PublicId,
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
