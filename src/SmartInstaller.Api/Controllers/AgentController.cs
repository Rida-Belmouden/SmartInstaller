using Microsoft.AspNetCore.Mvc;
using SmartInstaller.Services.Agent.DTOs;
using SmartInstaller.Services.Agent.Queries.CheckUpdates;
using SmartInstaller.Services.Agent.Queries.GetAgentCatalog;
using SmartInstaller.Services.Agent.Queries.GetInstallerManifest;
using SmartInstaller.Services.Common.Models;

namespace SmartInstaller.Api.Controllers;

[ApiController]
[Route("api/agent")]
public sealed class AgentController(
    IGetAgentCatalogHandler getCatalogHandler,
    ICheckUpdatesHandler checkUpdatesHandler,
    IGetInstallerManifestHandler getInstallerManifestHandler)
    : ControllerBase
{
    [HttpGet("catalog")]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<AgentCatalogItemDto>>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<ApiResponse<IReadOnlyList<AgentCatalogItemDto>>>> GetCatalog(
        [FromQuery] string? architecture,
        CancellationToken cancellationToken)
    {
        var catalog = await getCatalogHandler.HandleAsync(
            new GetAgentCatalogQuery(architecture),
            cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<AgentCatalogItemDto>>.Ok(catalog));
    }

    [HttpPost("check-updates")]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<UpdateCheckItemDto>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<UpdateCheckItemDto>>),
        StatusCodes.Status400BadRequest)]
    public async Task<
        ActionResult<ApiResponse<IReadOnlyList<UpdateCheckItemDto>>>> CheckUpdates(
        [FromBody] CheckUpdatesRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Architecture))
        {
            return BadRequest(
                ApiResponse<IReadOnlyList<UpdateCheckItemDto>>.Failure(
                    "Architecture is required."));
        }

        if (request.Applications is null)
        {
            return BadRequest(
                ApiResponse<IReadOnlyList<UpdateCheckItemDto>>.Failure(
                    "Applications are required."));
        }

        if (request.Applications.Any(application =>
                application.ApplicationId == Guid.Empty ||
                string.IsNullOrWhiteSpace(application.InstalledVersion)))
        {
            return BadRequest(
                ApiResponse<IReadOnlyList<UpdateCheckItemDto>>.Failure(
                    "Each installed application requires an application ID and version."));
        }

        var updates = await checkUpdatesHandler.HandleAsync(
            new CheckUpdatesQuery(
                request.Architecture.Trim(),
                request.Applications),
            cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<UpdateCheckItemDto>>.Ok(updates));
    }

    [HttpGet("installer-manifest/{installerProfileId:guid}")]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerManifestDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<InstallerManifestDto>),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InstallerManifestDto>>>
        GetInstallerManifest(
            Guid installerProfileId,
            CancellationToken cancellationToken)
    {
        var result = await getInstallerManifestHandler.HandleAsync(
            new GetInstallerManifestQuery(installerProfileId),
            cancellationToken);

        return result.Status switch
        {
            GetInstallerManifestStatus.Success =>
                Ok(ApiResponse<InstallerManifestDto>.Ok(result.Manifest!)),

            GetInstallerManifestStatus.NotFound =>
                NotFound(ApiResponse<InstallerManifestDto>.Failure(
                    "Installer manifest was not found.")),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<InstallerManifestDto>.Failure(
                    "An unexpected error occurred."))
        };
    }
}
