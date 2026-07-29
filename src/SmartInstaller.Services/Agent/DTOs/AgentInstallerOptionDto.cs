namespace SmartInstaller.Services.Agent.DTOs;

public sealed record AgentInstallerOptionDto(
    Guid InstallerProfileId,
    string InstallerType,
    string Architecture,
    long? FileSizeBytes,
    bool RequiresAdministrator,
    bool IsPortable);
