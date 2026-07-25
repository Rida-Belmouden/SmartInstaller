using SmartInstaller.Core.Common;

namespace SmartInstaller.Core.Entities.Installer;

public class InstallerType : BaseEntity
{
    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public ICollection<InstallerProfile> InstallerProfiles { get; set; }
        = new List<InstallerProfile>();
}