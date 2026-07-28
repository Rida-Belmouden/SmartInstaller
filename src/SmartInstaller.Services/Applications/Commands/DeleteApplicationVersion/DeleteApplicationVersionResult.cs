namespace SmartInstaller.Services.Applications
    .Commands.DeleteApplicationVersion;

public enum DeleteApplicationVersionStatus
{
    Success,
    VersionNotFound
}

public sealed record DeleteApplicationVersionResult(
    DeleteApplicationVersionStatus Status);