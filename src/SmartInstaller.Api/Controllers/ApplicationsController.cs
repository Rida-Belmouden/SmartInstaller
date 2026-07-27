using Microsoft.AspNetCore.Mvc;
using SmartInstaller.Services.Applications.DTOs;
using SmartInstaller.Services.Applications.Queries.GetApplicationById;
using SmartInstaller.Services.Applications.Queries.GetApplications;
using SmartInstaller.Services.Common.Models;

namespace SmartInstaller.Api.Controllers;

[ApiController]
[Route("api/applications")]
public sealed class ApplicationsController(
    IGetApplicationsHandler getApplicationsHandler,
    IGetApplicationByIdHandler getApplicationByIdHandler)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<ApplicationListItemDto>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        ApiResponse<PagedResult<ApplicationListItemDto>>>> GetAll(
        [FromQuery] GetApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await getApplicationsHandler.HandleAsync(
            query,
            cancellationToken);

        return Ok(
            ApiResponse<PagedResult<ApplicationListItemDto>>
                .Ok(result));
    }

    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(
        typeof(ApiResponse<ApplicationDetailsDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<ApplicationDetailsDto>),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<
        ApiResponse<ApplicationDetailsDto>>> GetById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var application =
            await getApplicationByIdHandler.HandleAsync(
                new GetApplicationByIdQuery(publicId),
                cancellationToken);

        if (application is null)
        {
            return NotFound(
                ApiResponse<ApplicationDetailsDto>.Failure(
                    "Application was not found."));
        }

        return Ok(
            ApiResponse<ApplicationDetailsDto>.Ok(application));
    }
}