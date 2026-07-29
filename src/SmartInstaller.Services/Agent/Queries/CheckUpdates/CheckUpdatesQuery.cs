using SmartInstaller.Services.Agent.DTOs;

namespace SmartInstaller.Services.Agent.Queries.CheckUpdates;

public sealed record CheckUpdatesQuery(
    string Architecture,
    IReadOnlyList<InstalledApplicationRequest> Applications);
