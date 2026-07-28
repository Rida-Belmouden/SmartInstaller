using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications
    .Commands.SetLatestApplicationVersion;

public sealed record SetLatestApplicationVersionResult(
    SetLatestApplicationVersionStatus Status,
    ApplicationVersionDto? Version = null);