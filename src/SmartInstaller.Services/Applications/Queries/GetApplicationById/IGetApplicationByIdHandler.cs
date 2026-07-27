using SmartInstaller.Services.Applications.DTOs;

namespace SmartInstaller.Services.Applications.Queries.GetApplicationById;

public interface IGetApplicationByIdHandler
{
    Task<ApplicationDetailsDto?> HandleAsync(
        GetApplicationByIdQuery query,
        CancellationToken cancellationToken = default);
}