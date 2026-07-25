namespace SmartInstaller.Core.Entities.Catalog;

public class ApplicationTag
{
    public int SoftwareApplicationId { get; set; }

    public int TagId { get; set; }

    public SoftwareApplication SoftwareApplication { get; set; } = null!;

    public Tag Tag { get; set; } = null!;
}