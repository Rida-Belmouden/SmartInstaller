using Microsoft.AspNetCore.Mvc;
using SmartInstaller.Web.Models.AdminCatalog;
using SmartInstaller.Web.Services;

namespace SmartInstaller.Web.Controllers;

[Route("admin/catalog")]
public sealed class AdminCatalogController(
    IAdminCatalogApiClient apiClient)
    : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? search,
        CancellationToken cancellationToken)
    {
        var applications = await apiClient.GetApplicationsAsync(
            search,
            cancellationToken);

        return View(new AdminCatalogIndexViewModel
        {
            Applications = applications,
            Search = search
        });
    }

    [HttpGet("applications/{publicId:guid}")]
    public async Task<IActionResult> Application(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var application = await apiClient.GetApplicationAsync(
            publicId,
            cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        var versions = await apiClient.GetVersionsAsync(
            publicId,
            cancellationToken)
            ?? Array.Empty<ApplicationVersionItem>();

        var profiles = new Dictionary<
            Guid,
            IReadOnlyList<InstallerProfileItem>>();

        foreach (var version in versions)
        {
            profiles[version.PublicId] =
                await apiClient.GetInstallerProfilesAsync(
                    version.PublicId,
                    cancellationToken);
        }

        return View(new AdminApplicationViewModel
        {
            Application = application,
            Versions = versions,
            ProfilesByVersion = profiles
        });
    }

    [HttpGet("applications/{applicationId:guid}/versions/create")]
    public async Task<IActionResult> CreateVersion(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var application = await apiClient.GetApplicationAsync(
            applicationId,
            cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        return View(new CreateVersionViewModel
        {
            ApplicationPublicId = application.PublicId,
            ApplicationName = application.Name,
            ReleaseDate = DateTime.UtcNow.Date,
            IsLatest = true
        });
    }

    [HttpPost("applications/{applicationId:guid}/versions/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVersion(
        Guid applicationId,
        CreateVersionViewModel model,
        CancellationToken cancellationToken)
    {
        model.ApplicationPublicId = applicationId;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await apiClient.CreateVersionAsync(
            model,
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message ?? "Unable to create the version.");

            return View(model);
        }

        TempData["Success"] = "Application version created successfully.";

        return RedirectToAction(
            nameof(Application),
            new { publicId = applicationId });
    }

    [HttpGet(
        "applications/{applicationId:guid}/versions/{versionId:guid}/profiles/create")]
    public async Task<IActionResult> CreateInstallerProfile(
        Guid applicationId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var application = await apiClient.GetApplicationAsync(
            applicationId,
            cancellationToken);

        var versions = await apiClient.GetVersionsAsync(
            applicationId,
            cancellationToken);

        var version = versions?.FirstOrDefault(
            item => item.PublicId == versionId);

        if (application is null || version is null)
        {
            return NotFound();
        }

        ViewBag.InstallerTypes = CatalogReferenceData.InstallerTypes;
        ViewBag.Architectures = CatalogReferenceData.Architectures;

        return View(new CreateInstallerProfileViewModel
        {
            ApplicationPublicId = applicationId,
            ApplicationName = application.Name,
            ApplicationVersionPublicId = versionId,
            Version = version.Version,
            InstallerTypePublicId =
                CatalogReferenceData.InstallerTypes[0].PublicId,
            ArchitecturePublicId =
                CatalogReferenceData.Architectures[1].PublicId,
            RequiresAdministrator = true,
            IsEnabled = true
        });
    }

    [HttpPost(
        "applications/{applicationId:guid}/versions/{versionId:guid}/profiles/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInstallerProfile(
        Guid applicationId,
        Guid versionId,
        CreateInstallerProfileViewModel model,
        CancellationToken cancellationToken)
    {
        model.ApplicationPublicId = applicationId;
        model.ApplicationVersionPublicId = versionId;

        ViewBag.InstallerTypes = CatalogReferenceData.InstallerTypes;
        ViewBag.Architectures = CatalogReferenceData.Architectures;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await apiClient.CreateInstallerProfileAsync(
            model,
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message ??
                "Unable to create the installer profile.");

            return View(model);
        }

        TempData["Success"] = "Installer profile created successfully.";

        return RedirectToAction(
            nameof(Application),
            new { publicId = applicationId });
    }

    [HttpPost("profiles/{profileId:guid}/deactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateProfile(
        Guid profileId,
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.DeactivateInstallerProfileAsync(
            profileId,
            cancellationToken);

        TempData[result.Success ? "Success" : "Error"] =
            result.Success
                ? "Installer profile deactivated."
                : result.Message;

        return RedirectToAction(
            nameof(Application),
            new { publicId = applicationId });
    }
}
