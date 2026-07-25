namespace SmartInstaller.Core.Entities;

public class InstallerProfile
{
    public int Id { get; set; }

    public int ApplicationVersionId { get; set; }

    public int InstallerTypeId { get; set; }

    public int ArchitectureId { get; set; }

    public string DownloadUrl { get; set; } = string.Empty;

    public string? Sha256 { get; set; }

    public long? FileSizeBytes { get; set; }

    public string? SilentInstallArguments { get; set; }

    public string? SilentUninstallArguments { get; set; }

    public bool RequiresAdministrator { get; set; } = true;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ApplicationVersion ApplicationVersion { get; set; } = null!;

    public InstallerType InstallerType { get; set; } = null!;

    public Architecture Architecture { get; set; } = null!;
}