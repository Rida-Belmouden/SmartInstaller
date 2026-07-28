using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications
    .Queries.GetApplicationVersions;

public interface IGetApplicationVersionsHandler
{
    Task<IReadOnlyList<ApplicationVersionDto>?> HandleAsync(
        GetApplicationVersionsQuery query,
        CancellationToken cancellationToken = default);
}