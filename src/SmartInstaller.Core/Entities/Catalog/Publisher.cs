using SmartInstaller.Core.Common;

namespace SmartInstaller.Core.Entities.Catalog;

public class Publisher : BaseEntity
{
    public string Name { get; set; } = "";

    public string? Website { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsVerified { get; set; }

    public ICollection<SoftwareApplication> Applications { get; set; }
        = new List<SoftwareApplication>();
}