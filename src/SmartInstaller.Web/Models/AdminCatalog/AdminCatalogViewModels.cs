using System.ComponentModel.DataAnnotations;

namespace SmartInstaller.Web.Models.AdminCatalog;

public sealed class AdminCatalogIndexViewModel
{
    public IReadOnlyList<ApplicationListItem> Applications { get; init; }
        = Array.Empty<ApplicationListItem>();

    public string? Search { get; init; }
}

public sealed class AdminApplicationViewModel
{
    public ApplicationDetails Application { get; init; } = null!;

    public IReadOnlyList<ApplicationVersionItem> Versions { get; init; }
        = Array.Empty<ApplicationVersionItem>();

    public IReadOnlyDictionary<Guid, IReadOnlyList<InstallerProfileItem>>
        ProfilesByVersion { get; init; }
        = new Dictionary<Guid, IReadOnlyList<InstallerProfileItem>>();
}

public sealed class CreateVersionViewModel
{
    public Guid ApplicationPublicId { get; set; }

    public string ApplicationName { get; set; } = "";

    [Required]
    [StringLength(50)]
    public string Version { get; set; } = "";

    [DataType(DataType.Date)]
    public DateTime? ReleaseDate { get; set; }

    public bool IsLatest { get; set; } = true;
}

public sealed class CreateInstallerProfileViewModel
{
    public Guid ApplicationPublicId { get; set; }

    public string ApplicationName { get; set; } = "";

    public Guid ApplicationVersionPublicId { get; set; }

    public string Version { get; set; } = "";

    [Required]
    public Guid InstallerTypePublicId { get; set; }

    [Required]
    public Guid ArchitecturePublicId { get; set; }

    [Required]
    [Url]
    public string DownloadUrl { get; set; } = "";

    [RegularExpression(
        "^[a-fA-F0-9]{64}$",
        ErrorMessage = "SHA-256 must contain 64 hexadecimal characters.")]
    public string? Sha256 { get; set; }

    [Range(0, long.MaxValue)]
    public long? FileSizeBytes { get; set; }

    public string? SilentInstallArguments { get; set; }

    public string? SilentUninstallArguments { get; set; }

    public bool RequiresAdministrator { get; set; } = true;

    public bool IsPortable { get; set; }

    public bool IsEnabled { get; set; } = true;
}

public static class CatalogReferenceData
{
    public static readonly IReadOnlyList<ReferenceOption> InstallerTypes =
    [
        new(
            Guid.Parse("30000000-0000-0000-0000-000000000001"),
            "EXE"),
        new(
            Guid.Parse("30000000-0000-0000-0000-000000000002"),
            "MSI"),
        new(
            Guid.Parse("30000000-0000-0000-0000-000000000003"),
            "MSIX"),
        new(
            Guid.Parse("30000000-0000-0000-0000-000000000004"),
            "ZIP")
    ];

    public static readonly IReadOnlyList<ReferenceOption> Architectures =
    [
        new(
            Guid.Parse("20000000-0000-0000-0000-000000000001"),
            "x86"),
        new(
            Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "x64"),
        new(
            Guid.Parse("20000000-0000-0000-0000-000000000003"),
            "ARM64"),
        new(
            Guid.Parse("20000000-0000-0000-0000-000000000004"),
            "Any")
    ];
}

public sealed record ReferenceOption(Guid PublicId, string Name);
