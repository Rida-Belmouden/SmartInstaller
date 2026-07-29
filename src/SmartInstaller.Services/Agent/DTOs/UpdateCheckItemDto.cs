namespace SmartInstaller.Services.Agent.DTOs;

public sealed record UpdateCheckItemDto(
    Guid ApplicationId,
    string ApplicationName,
    string InstalledVersion,
    string LatestVersion,
    bool UpdateAvailable,
    Guid? InstallerProfileId);
