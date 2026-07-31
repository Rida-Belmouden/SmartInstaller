using SmartInstaller.Web.Models.AdminCatalog;

namespace SmartInstaller.Web.Services;

public interface IAdminCatalogApiClient
{
    Task<IReadOnlyList<ApplicationListItem>> GetApplicationsAsync(
        string? search,
        CancellationToken cancellationToken);

    Task<ApplicationDetails?> GetApplicationAsync(
        Guid publicId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ApplicationVersionItem>?> GetVersionsAsync(
        Guid applicationPublicId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<InstallerProfileItem>> GetInstallerProfilesAsync(
        Guid applicationVersionPublicId,
        CancellationToken cancellationToken);

    Task<(bool Success, string? Message)> CreateVersionAsync(
        CreateVersionViewModel model,
        CancellationToken cancellationToken);

    Task<(bool Success, string? Message)> CreateInstallerProfileAsync(
        CreateInstallerProfileViewModel model,
        CancellationToken cancellationToken);

    Task<(bool Success, string? Message)> DeactivateInstallerProfileAsync(
        Guid profilePublicId,
        CancellationToken cancellationToken);
}
