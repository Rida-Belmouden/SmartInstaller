using SmartInstaller.Core.Common;

namespace SmartInstaller.Core.Entities.Catalog;

public class Platform : BaseEntity
{
    public string Name { get; set; } = "";

    public string Slug { get; set; } = "";

    public string? Description { get; set; }

    public ICollection<SoftwareApplication> Applications { get; set; }
        = new List<SoftwareApplication>();
}