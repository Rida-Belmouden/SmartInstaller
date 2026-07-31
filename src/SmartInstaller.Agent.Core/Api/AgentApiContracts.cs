namespace SmartInstaller.Agent.Core.Api;

internal sealed record CheckUpdatesRequest(
    string Architecture,
    IReadOnlyList<InstalledApplicationRequest> Applications);

internal sealed record InstalledApplicationRequest(
    Guid ApplicationId,
    string InstalledVersion);
