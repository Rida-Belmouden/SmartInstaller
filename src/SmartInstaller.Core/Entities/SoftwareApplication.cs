namespace SmartInstaller.Core.Entities;

public class SoftwareApplication
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public int PublisherId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Website { get; set; }

    public string? IconUrl { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public Category Category { get; set; } = null!;

    public Publisher Publisher { get; set; } = null!;

    public ICollection<ApplicationVersion> Versions { get; set; }
        = new List<ApplicationVersion>();
}