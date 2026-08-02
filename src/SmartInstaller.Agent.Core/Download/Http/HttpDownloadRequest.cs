using SmartInstaller.Agent.Core.Download.Models;

namespace SmartInstaller.Agent.Core.Download.Http;

public sealed record HttpDownloadRequest(
    Uri DownloadUrl,
    string DestinationPath,
    long? ExpectedFileSizeBytes,
    IProgress<DownloadProgress>? Progress,
    long ResumeOffsetBytes = 0);
