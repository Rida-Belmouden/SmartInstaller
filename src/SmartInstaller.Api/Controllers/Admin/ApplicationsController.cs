using Microsoft.AspNetCore.Mvc;
using SmartInstaller.Services.Applications
    .Commands.CreateApplicationVersion;
using SmartInstaller.Services.Applications.DTOs;
using SmartInstaller.Services.Common.Models;

namespace SmartInstaller.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/applications")]
public sealed class ApplicationsController(
    ICreateApplicationVersionHandler createApplicationVersionHandler)
    : ControllerBase
{
    [HttpPost("{publicId:guid}/versions")]
    [ProducesResponseType(
        typeof(ApiResponse<ApplicationVersionDto>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiResponse<ApplicationVersionDto>),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiResponse<ApplicationVersionDto>),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiResponse<ApplicationVersionDto>),
        StatusCodes.Status409Conflict)]
    public async Task<
        ActionResult<ApiResponse<ApplicationVersionDto>>> CreateVersion(
        Guid publicId,
        [FromBody] CreateApplicationVersionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateApplicationVersionCommand(
            publicId,
            request.Version,
            request.ReleaseDate,
            request.IsLatest);

        var result =
            await createApplicationVersionHandler.HandleAsync(
                command,
                cancellationToken);

        return result.Status switch
        {
            CreateApplicationVersionStatus.Success =>
                Created(
                    $"/api/versions/{result.Version!.PublicId}",
                    ApiResponse<ApplicationVersionDto>.Ok(
                        result.Version,
                        "Application version was created successfully.")),

            CreateApplicationVersionStatus.ApplicationNotFound =>
                NotFound(
                    ApiResponse<ApplicationVersionDto>.Failure(
                        "Application was not found.")),

            CreateApplicationVersionStatus.DuplicateVersion =>
                Conflict(
                    ApiResponse<ApplicationVersionDto>.Failure(
                        "This application version already exists.")),

            CreateApplicationVersionStatus.InvalidVersion =>
                BadRequest(
                    ApiResponse<ApplicationVersionDto>.Failure(
                        "Version is required.")),

            _ =>
                StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<ApplicationVersionDto>.Failure(
                        "An unexpected error occurred."))
        };
    }
}