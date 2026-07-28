using Microsoft.AspNetCore.Mvc;
using SmartInstaller.Services.Applications.DTOs;
using SmartInstaller.Services.Applications
    .Queries.GetApplicationVersionById;
using SmartInstaller.Services.Common.Models;

namespace SmartInstaller.Api.Controllers;

[ApiController]
[Route("api/versions")]
public sealed class VersionsController(
    IGetApplicationVersionByIdHandler getVersionByIdHandler)
    : ControllerBase
{
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(
        typeof(ApiResponse<ApplicationVersionDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<ApplicationVersionDto>),
        StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<ApiResponse<ApplicationVersionDto>>> GetById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var version = await getVersionByIdHandler.HandleAsync(
            new GetApplicationVersionByIdQuery(publicId),
            cancellationToken);

        if (version is null)
        {
            return NotFound(
                ApiResponse<ApplicationVersionDto>.Failure(
                    "Application version was not found."));
        }

        return Ok(
            ApiResponse<ApplicationVersionDto>.Ok(version));
    }
}