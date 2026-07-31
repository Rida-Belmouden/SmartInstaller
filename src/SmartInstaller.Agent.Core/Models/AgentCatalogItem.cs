namespace SmartInstaller.Agent.Core.Models;

public sealed record AgentCatalogItem(
    Guid ApplicationId,
    string Name,
    string Publisher,
    Guid LatestVersionId,
    string LatestVersion,
    DateTime? ReleaseDate,
    IReadOnlyList<AgentInstallerOption> Installers);

public sealed record AgentInstallerOption(
    Guid InstallerProfileId,
    string InstallerType,
    string Architecture,
    long? FileSizeBytes,
    bool RequiresAdministrator,
    bool IsPortable);
