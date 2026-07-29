using SmartInstaller.Services.Agent.DTOs;

namespace SmartInstaller.Services.Agent.Queries.CheckUpdates;

public interface ICheckUpdatesHandler
{
    Task<IReadOnlyList<UpdateCheckItemDto>> HandleAsync(
        CheckUpdatesQuery query,
        CancellationToken cancellationToken = default);
}
