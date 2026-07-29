namespace SmartInstaller.Services.Agent.DTOs;

public sealed record AgentCatalogItemDto(
    Guid ApplicationId,
    string Name,
    string Publisher,
    Guid LatestVersionId,
    string LatestVersion,
    DateTime? ReleaseDate,
    IReadOnlyList<AgentInstallerOptionDto> Installers);
