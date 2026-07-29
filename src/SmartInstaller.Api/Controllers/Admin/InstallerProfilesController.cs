using Microsoft.AspNetCore.Mvc;
using SmartInstaller.Services.Common.Models;
using SmartInstaller.Services.InstallerProfiles.Commands.CreateInstallerProfile;
using SmartInstaller.Services.InstallerProfiles.Commands.DeactivateInstallerProfile;
using SmartInstaller.Services.InstallerProfiles.Commands.UpdateInstallerProfile;
using SmartInstaller.Services.InstallerProfiles.DTOs;

namespace SmartInstaller.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/installer-profiles")]
public sealed class InstallerProfilesController(
    ICreateInstallerProfileHandler createHandler,
    IUpdateInstallerProfileHandler updateHandler,
    IDeactivateInstallerProfileHandler deactivateHandler)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<InstallerProfileDto>>> Create(
        [FromBody] CreateInstallerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateInstallerProfileCommand(
            request.ApplicationVersionPublicId,
            request.InstallerTypePublicId,
            request.ArchitecturePublicId,
            request.DownloadUrl,
            request.Sha256,
            request.FileSizeBytes,
            request.SilentInstallArguments,
            request.SilentUninstallArguments,
            request.RequiresAdministrator,
            request.IsPortable,
            request.IsEnabled);

        var result = await createHandler.HandleAsync(
            command,
            cancellationToken);

        return result.Status switch
        {
            CreateInstallerProfileStatus.Success =>
                Created(
                    $"/api/installer-profiles/{result.InstallerProfile!.PublicId}",
                    ApiResponse<InstallerProfileDto>.Ok(
                        result.InstallerProfile,
                        "Installer profile was created successfully.")),

            CreateInstallerProfileStatus.ApplicationVersionNotFound =>
                NotFound(ApiResponse<InstallerProfileDto>.Failure(
                    "Application version was not found.")),

            CreateInstallerProfileStatus.InstallerTypeNotFound =>
                NotFound(ApiResponse<InstallerProfileDto>.Failure(
                    "Installer type was not found.")),

            CreateInstallerProfileStatus.ArchitectureNotFound =>
                NotFound(ApiResponse<InstallerProfileDto>.Failure(
                    "Architecture was not found.")),

            CreateInstallerProfileStatus.DuplicateInstallerProfile =>
                Conflict(ApiResponse<InstallerProfileDto>.Failure(
                    "This installer profile already exists.")),

            CreateInstallerProfileStatus.InvalidDownloadUrl =>
                BadRequest(ApiResponse<InstallerProfileDto>.Failure(
                    "Download URL must be an absolute HTTP or HTTPS URL.")),

            CreateInstallerProfileStatus.InvalidSha256 =>
                BadRequest(ApiResponse<InstallerProfileDto>.Failure(
                    "SHA-256 must contain exactly 64 hexadecimal characters.")),

            CreateInstallerProfileStatus.InvalidFileSize =>
                BadRequest(ApiResponse<InstallerProfileDto>.Failure(
                    "File size cannot be negative.")),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<InstallerProfileDto>.Failure(
                    "An unexpected error occurred."))
        };
    }

    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<InstallerProfileDto>>> Update(
        Guid publicId,
        [FromBody] UpdateInstallerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateInstallerProfileCommand(
            publicId,
            request.InstallerTypePublicId,
            request.ArchitecturePublicId,
            request.DownloadUrl,
            request.Sha256,
            request.FileSizeBytes,
            request.SilentInstallArguments,
            request.SilentUninstallArguments,
            request.RequiresAdministrator,
            request.IsPortable,
            request.IsEnabled);

        var result = await updateHandler.HandleAsync(
            command,
            cancellationToken);

        return result.Status switch
        {
            UpdateInstallerProfileStatus.Success =>
                Ok(ApiResponse<InstallerProfileDto>.Ok(
                    result.InstallerProfile!,
                    "Installer profile was updated successfully.")),

            UpdateInstallerProfileStatus.InstallerProfileNotFound =>
                NotFound(ApiResponse<InstallerProfileDto>.Failure(
                    "Installer profile was not found.")),

            UpdateInstallerProfileStatus.InstallerTypeNotFound =>
                NotFound(ApiResponse<InstallerProfileDto>.Failure(
                    "Installer type was not found.")),

            UpdateInstallerProfileStatus.ArchitectureNotFound =>
                NotFound(ApiResponse<InstallerProfileDto>.Failure(
                    "Architecture was not found.")),

            UpdateInstallerProfileStatus.DuplicateInstallerProfile =>
                Conflict(ApiResponse<InstallerProfileDto>.Failure(
                    "This installer profile already exists.")),

            UpdateInstallerProfileStatus.InvalidDownloadUrl =>
                BadRequest(ApiResponse<InstallerProfileDto>.Failure(
                    "Download URL must be an absolute HTTP or HTTPS URL.")),

            UpdateInstallerProfileStatus.InvalidSha256 =>
                BadRequest(ApiResponse<InstallerProfileDto>.Failure(
                    "SHA-256 must contain exactly 64 hexadecimal characters.")),

            UpdateInstallerProfileStatus.InvalidFileSize =>
                BadRequest(ApiResponse<InstallerProfileDto>.Failure(
                    "File size cannot be negative.")),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<InstallerProfileDto>.Failure(
                    "An unexpected error occurred."))
        };
    }

    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await deactivateHandler.HandleAsync(
            new DeactivateInstallerProfileCommand(publicId),
            cancellationToken);

        return result.Status switch
        {
            DeactivateInstallerProfileStatus.Success => NoContent(),

            DeactivateInstallerProfileStatus.NotFound =>
                NotFound(ApiResponse<InstallerProfileDto>.Failure(
                    "Installer profile was not found.")),

            DeactivateInstallerProfileStatus.AlreadyInactive =>
                Conflict(ApiResponse<InstallerProfileDto>.Failure(
                    "Installer profile is already inactive.")),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<InstallerProfileDto>.Failure(
                    "An unexpected error occurred."))
        };
    }
}
