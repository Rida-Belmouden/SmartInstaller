namespace SmartInstaller.Agent.Core.Download.Models;

public sealed record DownloadRequest(
    Uri DownloadUrl,
    string FileName,
    string? Sha256 = null,
    long? ExpectedFileSizeBytes = null,
    bool Overwrite = false);
