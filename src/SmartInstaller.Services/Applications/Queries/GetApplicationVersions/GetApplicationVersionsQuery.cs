namespace SmartInstaller.Services.Applications
    .Queries.GetApplicationVersions;

public sealed record GetApplicationVersionsQuery(
    Guid ApplicationPublicId);