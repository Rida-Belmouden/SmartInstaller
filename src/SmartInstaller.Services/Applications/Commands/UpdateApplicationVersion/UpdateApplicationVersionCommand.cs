namespace SmartInstaller.Services.Applications
    .Commands.UpdateApplicationVersion;

public sealed record UpdateApplicationVersionCommand(
    Guid VersionPublicId,
    string Version,
    DateTime? ReleaseDate,
    bool IsLatest);