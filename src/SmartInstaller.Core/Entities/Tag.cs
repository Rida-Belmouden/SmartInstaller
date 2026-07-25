using SmartInstaller.Core.Common;
using SmartInstaller.Core.Entities.Catalog;

namespace SmartInstaller.Core.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = "";

    public string Slug { get; set; } = "";

    public string? Description { get; set; }

    public ICollection<ApplicationTag> ApplicationTags { get; set; }
        = new List<ApplicationTag>();
}