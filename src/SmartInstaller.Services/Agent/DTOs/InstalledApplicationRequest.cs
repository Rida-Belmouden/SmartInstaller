namespace SmartInstaller.Services.Agent.DTOs;

public sealed record InstalledApplicationRequest(
    Guid ApplicationId,
    string InstalledVersion);
