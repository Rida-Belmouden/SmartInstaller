using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications
    .Commands.UpdateApplicationVersion;

public enum UpdateApplicationVersionStatus
{
    Success,
    VersionNotFound,
    InvalidVersion,
    DuplicateVersion
}

public sealed record UpdateApplicationVersionResult(
    UpdateApplicationVersionStatus Status,
    ApplicationVersionDto? Version = null);