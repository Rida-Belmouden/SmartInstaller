using SmartInstaller.Services.Agent.DTOs;

namespace SmartInstaller.Services.Agent.Queries.GetInstallerManifest;

public sealed record GetInstallerManifestResult(
    GetInstallerManifestStatus Status,
    InstallerManifestDto? Manifest = null);
