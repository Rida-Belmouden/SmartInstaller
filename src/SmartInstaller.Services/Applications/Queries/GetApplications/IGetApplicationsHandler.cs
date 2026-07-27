using SmartInstaller.Services.Applications.DTOs;
using SmartInstaller.Services.Common.Models;

namespace SmartInstaller.Services.Applications.Queries.GetApplications;

public interface IGetApplicationsHandler
{
    Task<PagedResult<ApplicationListItemDto>> HandleAsync(
        GetApplicationsQuery query,
        CancellationToken cancellationToken = default);
}