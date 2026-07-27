using Microsoft.AspNetCore.Mvc;
using SmartInstaller.Services.Catalog.DTOs;
using SmartInstaller.Services.Catalog.Queries.GetCategories;
using SmartInstaller.Services.Common.Models;

namespace SmartInstaller.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(
    IGetCategoriesHandler handler)
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
        var categories = await handler.HandleAsync(
            cancellationToken);

        return Ok(
            ApiResponse<IReadOnlyCollection<CatalogItemDto>>
                .Ok(categories));
    }
}