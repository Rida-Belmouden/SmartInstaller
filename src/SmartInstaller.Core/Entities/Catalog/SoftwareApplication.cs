using SmartInstaller.Core.Common;
using SmartInstaller.Core.Entities.Installer;
using System.Runtime.InteropServices;

namespace SmartInstaller.Core.Entities.Catalog;

public class SoftwareApplication : BaseEntity
{
    public int CategoryId { get; set; }

    public int PublisherId { get; set; }

    public int PlatformId { get; set; }

    public string Name { get; set; } = "";

    public string Slug { get; set; } = "";

    public string? Description { get; set; }

    public string? Website { get; set; }

    public string? IconUrl { get; set; }

    public bool IsFeatured { get; set; }

    public Category Category { get; set; } = null!;

    public Publisher Publisher { get; set; } = null!;

    public Platform Platform { get; set; } = null!;

    public ICollection<ApplicationVersion> Versions { get; set; }
        = new List<ApplicationVersion>();

    public ICollection<ApplicationTag> ApplicationTags { get; set; }
        = new List<ApplicationTag>();
}