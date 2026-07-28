using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications
    .Commands.CreateApplicationVersion;

public sealed record CreateApplicationVersionResult(
    CreateApplicationVersionStatus Status,
    ApplicationVersionDto? Version = null);

public enum CreateApplicationVersionStatus
{
    Success,
    ApplicationNotFound,
    DuplicateVersion,
    InvalidVersion
}