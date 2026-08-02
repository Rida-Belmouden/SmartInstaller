using Microsoft.AspNetCore.Mvc;
using SmartInstaller.Services.Common.Models;
using SmartInstaller.Services.InstallerProfiles.DTOs;
using SmartInstaller.Services.InstallerProfiles.Queries.GetInstallerProfileById;
using SmartInstaller.Services.InstallerProfiles.Queries.GetInstallerProfiles;

namespace SmartInstaller.Api.Controllers;

[ApiController]
[Route("api/installer-profiles")]
public sealed class InstallerProfilesController(
    IGetInstallerProfilesHandler getInstallerProfilesHandler,
    IGetInstallerProfileByIdHandler getInstallerProfileByIdHandler)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<InstallerProfileDto>>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<ApiResponse<IReadOnlyList<InstallerProfileDto>>>> GetAll(
        [FromQuery(Name = "applicationVersionPublicId")]
    Guid? applicationVersionPublicId,

        [FromQuery(Name = "installerTypePublicId")]
    Guid? installerTypePublicId,

        [FromQuery(Name = "architecturePublicId")]
    Guid? architecturePublicId,

        [FromQuery]
    bool? isEnabled,

        CancellationToken cancellationToken)
    {
        var query = new GetInstallerProfilesQuery(
            applicationVersionPublicId,
            installerTypePublicId,
            architecturePublicId,
            isEnabled);

        var profiles =
            await getInstallerProfilesHandler.HandleAsync(
                query,
                cancellationToken);

        return Ok(
            ApiResponse<IReadOnlyList<InstallerProfileDto>>
                .Ok(profiles));
    }

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerProfileDto>),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InstallerProfileDto>>> GetById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await getInstallerProfileByIdHandler.HandleAsync(
            new GetInstallerProfileByIdQuery(publicId),
            cancellationToken);

        return result.Status switch
        {
            GetInstallerProfileByIdStatus.Success =>
                Ok(ApiResponse<InstallerProfileDto>.Ok(
                    result.InstallerProfile!)),

            GetInstallerProfileByIdStatus.NotFound =>
                NotFound(ApiResponse<InstallerProfileDto>.Failure(
                    "Installer profile was not found.")),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<InstallerProfileDto>.Failure(
                    "An unexpected error occurred."))
        };
    }
}
