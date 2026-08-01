using SmartInstaller.Agent.Core.Download.Models;

namespace SmartInstaller.Agent.Core.Models;

public sealed record UpdateDownloadResult(
    UpdateCheckItem Update,
    InstallerManifest? Manifest,
    DownloadResult DownloadResult);
