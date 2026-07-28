namespace SmartInstaller.Services.Applications
    .Commands.SetLatestApplicationVersion;

public sealed record SetLatestApplicationVersionCommand(
    Guid VersionPublicId);