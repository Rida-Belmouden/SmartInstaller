using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications
    .Queries.GetApplicationVersionById;

public interface IGetApplicationVersionByIdHandler
{
    Task<ApplicationVersionDto?> HandleAsync(
        GetApplicationVersionByIdQuery query,
        CancellationToken cancellationToken = default);
}