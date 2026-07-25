using SmartInstaller.Core.Common;

namespace SmartInstaller.Core.Entities.Catalog;

public class Tag : BaseEntity
{
    public string Name { get; set; } = "";

    public string Slug { get; set; } = "";

    public string? Description { get; set; }

    public ICollection<ApplicationTag> ApplicationTags { get; set; }
        = new List<ApplicationTag>();
}