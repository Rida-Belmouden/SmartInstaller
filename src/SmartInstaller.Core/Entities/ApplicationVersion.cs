namespace SmartInstaller.Core.Entities;

public class ApplicationVersion
{
    public int Id { get; set; }

    public int SoftwareApplicationId { get; set; }

    public string Version { get; set; } = string.Empty;

    public DateTime? ReleaseDate { get; set; }

    public bool IsLatest { get; set; }

    public bool IsActive { get; set; } = true;

    public SoftwareApplication SoftwareApplication { get; set; } = null!;

    public ICollection<InstallerProfile> InstallerProfiles { get; set; }
        = new List<InstallerProfile>();
}