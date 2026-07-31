namespace SmartInstaller.Agent.Core.Models;

public sealed record UpdateCheckItem(
    Guid ApplicationId,
    string ApplicationName,
    string InstalledVersion,
    string LatestVersion,
    bool UpdateAvailable,
    Guid? InstallerProfileId);
