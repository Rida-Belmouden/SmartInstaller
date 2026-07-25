using SmartInstaller.Core.Common;
using SmartInstaller.Core.Entities.Installer;

namespace SmartInstaller.Core.Entities.Catalog;

public class ApplicationVersion : BaseEntity
{
    public int SoftwareApplicationId { get; set; }

    public string Version { get; set; } = "";

    public DateTime? ReleaseDate { get; set; }

    public bool IsLatest { get; set; }

    public SoftwareApplication SoftwareApplication { get; set; } = null!;

    public ICollection<InstallerProfile> InstallerProfiles { get; set; }
        = new List<InstallerProfile>();
}