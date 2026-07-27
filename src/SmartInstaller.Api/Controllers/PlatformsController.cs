using Microsoft.AspNetCore.Mvc;
using SmartInstaller.Services.Catalog.DTOs;
using SmartInstaller.Services.Catalog.Queries.GetPlatforms;
using SmartInstaller.Services.Common.Models;

namespace SmartInstaller.Api.Controllers;

[ApiController]
[Route("api/platforms")]
public sealed class PlatformsController(
    IGetPlatformsHandler handler)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyCollection<CatalogItemDto>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        ApiResponse<IReadOnlyCollection<CatalogItemDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var platforms = await handler.HandleAsync(
            cancellationToken);

        return Ok(
            ApiResponse<IReadOnlyCollection<CatalogItemDto>>
                .Ok(platforms));
    }
}