namespace SmartInstaller.Services.Applications
    .Commands.DeleteApplicationVersion;

public sealed record DeleteApplicationVersionCommand(
    Guid VersionPublicId);