using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Download.Session;

public sealed record DownloadSessionItemState(
    UpdateCheckItem Update,
    string Status,
    double Percentage,
    bool IsDownloading,
    bool HasPartialDownload,
    long InitialPartialBytes,
    InstallerManifest? Manifest = null,
    string? FilePath = null,
    bool CanPause = false,
    bool CanResume = false,
    bool CanCancel = false);
