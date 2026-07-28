namespace SmartInstaller.Services.Applications
    .Commands.CreateApplicationVersion;

public sealed record CreateApplicationVersionCommand(
    Guid ApplicationPublicId,
    string Version,
    DateTime? ReleaseDate,
    bool IsLatest);