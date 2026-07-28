namespace SmartInstaller.Services.Applications
    .Queries.GetApplicationVersionById;

public sealed record GetApplicationVersionByIdQuery(
    Guid VersionPublicId);