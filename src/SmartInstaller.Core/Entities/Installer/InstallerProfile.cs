using SmartInstaller.Core.Common;
using SmartInstaller.Core.Entities.Catalog;

namespace SmartInstaller.Core.Entities.Installer;

public class InstallerProfile : BaseEntity
{
    public int ApplicationVersionId { get; set; }

    public int InstallerTypeId { get; set; }

    public int ArchitectureId { get; set; }

    public string DownloadUrl { get; set; } = "";

    public string? Sha256 { get; set; }

    public long? FileSizeBytes { get; set; }

    public string? SilentInstallArguments { get; set; }

    public string? SilentUninstallArguments { get; set; }

    public bool RequiresAdministrator { get; set; } = true;

    public ApplicationVersion ApplicationVersion { get; set; } = null!;

    public InstallerType InstallerType { get; set; } = null!;

    public Architecture Architecture { get; set; } = null!;
}