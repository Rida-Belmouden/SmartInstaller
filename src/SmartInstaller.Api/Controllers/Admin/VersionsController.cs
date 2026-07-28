using Microsoft.AspNetCore.Mvc;
using SmartInstaller.Services.Applications.Commands.DeleteApplicationVersion;
using SmartInstaller.Services.Applications
    .Commands.SetLatestApplicationVersion;
using SmartInstaller.Services.Applications.Commands.UpdateApplicationVersion;
using SmartInstaller.Services.Applications.DTOs;
using SmartInstaller.Services.Common.Models;

namespace SmartInstaller.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/versions")]
public sealed class VersionsController(
    ISetLatestApplicationVersionHandler setLatestHandler,
    IUpdateApplicationVersionHandler updateHandler,
    IDeleteApplicationVersionHandler deleteHandler)
    : ControllerBase
{
    [HttpPatch("{publicId:guid}/set-latest")]
    [ProducesResponseType(
        typeof(ApiResponse<ApplicationVersionDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiResponse<ApplicationVersionDto>),
        StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<ApiResponse<ApplicationVersionDto>>> SetLatest(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var command =
            new SetLatestApplicationVersionCommand(publicId);

        var result = await setLatestHandler.HandleAsync(
            command,
            cancellationToken);

        return result.Status switch
        {
            SetLatestApplicationVersionStatus.Success =>
                Ok(
                    ApiResponse<ApplicationVersionDto>.Ok(
                        result.Version!,
                        "Application version was set as latest.")),

            SetLatestApplicationVersionStatus.AlreadyLatest =>
                Ok(
                    ApiResponse<ApplicationVersionDto>.Ok(
                        result.Version!,
                        "Application version is already the latest.")),

            SetLatestApplicationVersionStatus.VersionNotFound =>
                NotFound(
                    ApiResponse<ApplicationVersionDto>.Failure(
                        "Application version was not found.")),

            _ =>
                StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<ApplicationVersionDto>.Failure(
                        "An unexpected error occurred."))
        };
    }

    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(
    typeof(ApiResponse<ApplicationVersionDto>),
    StatusCodes.Status200OK)]
    [ProducesResponseType(
    typeof(ApiResponse<ApplicationVersionDto>),
    StatusCodes.Status404NotFound)]
    [ProducesResponseType(
    typeof(ApiResponse<ApplicationVersionDto>),
    StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ApplicationVersionDto>>> Update(
    Guid publicId,
    UpdateApplicationVersionRequest request,
    CancellationToken cancellationToken)
    {
        var command = new UpdateApplicationVersionCommand(
            publicId,
            request.Version,
            request.ReleaseDate,
            request.IsLatest);

        var result = await updateHandler.HandleAsync(
            command,
            cancellationToken);

        return result.Status switch
        {
            UpdateApplicationVersionStatus.Success =>
                Ok(ApiResponse<ApplicationVersionDto>.Ok(
                    result.Version!,
                    "Application version updated.")),

            UpdateApplicationVersionStatus.VersionNotFound =>
                NotFound(ApiResponse<ApplicationVersionDto>.Failure(
                    "Application version was not found.")),

            UpdateApplicationVersionStatus.InvalidVersion =>
                BadRequest(ApiResponse<ApplicationVersionDto>.Failure(
                    "Version is invalid.")),

            UpdateApplicationVersionStatus.DuplicateVersion =>
                Conflict(ApiResponse<ApplicationVersionDto>.Failure(
                    "Version already exists.")),

            _ =>
                StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<ApplicationVersionDto>.Failure(
                        "Unexpected error."))
        };
    }

    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
    Guid publicId,
    CancellationToken cancellationToken)
    {
        var command =
            new DeleteApplicationVersionCommand(publicId);

        var result = await deleteHandler.HandleAsync(
            command,
            cancellationToken);

        return result.Status switch
        {
            DeleteApplicationVersionStatus.Success =>
                NoContent(),

            DeleteApplicationVersionStatus.VersionNotFound =>
                NotFound(),

            _ =>
                StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}