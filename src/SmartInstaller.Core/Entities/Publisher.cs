namespace SmartInstaller.Core.Entities;

public class Publisher
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Website { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsVerified { get; set; }

    // Navigation
    public ICollection<SoftwareApplication> Applications { get; set; }
        = new List<SoftwareApplication>();
}