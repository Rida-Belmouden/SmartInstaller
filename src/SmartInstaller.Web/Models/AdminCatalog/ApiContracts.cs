namespace SmartInstaller.Web.Models.AdminCatalog;

public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record ApplicationListItem(
    Guid PublicId,
    string Name,
    string Slug,
    string? Description,
    string? IconUrl,
    string Category,
    string Publisher,
    string Platform,
    string? LatestVersion,
    bool IsFeatured);

public sealed record ApplicationDetails(
    Guid PublicId,
    string Name,
    string Slug,
    string? Description,
    string? Website,
    string? IconUrl,
    bool IsFeatured,
    string Category,
    string Publisher,
    string Platform,
    IReadOnlyList<string> Tags,
    IReadOnlyList<ApplicationVersionItem> Versions);

public sealed record ApplicationVersionItem(
    Guid PublicId,
    string Version,
    DateTime? ReleaseDate,
    bool IsLatest,
    bool IsActive);

public sealed record InstallerProfileItem(
    Guid PublicId,
    Guid ApplicationVersionPublicId,
    Guid InstallerTypePublicId,
    string InstallerTypeName,
    Guid ArchitecturePublicId,
    string ArchitectureName,
    string DownloadUrl,
    string? Sha256,
    long? FileSizeBytes,
    string? SilentInstallArguments,
    string? SilentUninstallArguments,
    bool RequiresAdministrator,
    bool IsPortable,
    bool IsEnabled,
    bool IsActive);
