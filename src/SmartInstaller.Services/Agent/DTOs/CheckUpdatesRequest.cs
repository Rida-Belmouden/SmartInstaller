namespace SmartInstaller.Services.Agent.DTOs;

public sealed record CheckUpdatesRequest(
    string Architecture,
    IReadOnlyList<InstalledApplicationRequest> Applications);
