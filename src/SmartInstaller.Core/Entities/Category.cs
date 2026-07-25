namespace SmartInstaller.Core.Entities;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<SoftwareApplication> Applications { get; set; }
        = new List<SoftwareApplication>();
}