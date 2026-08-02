namespace SmartInstaller.Agent.Core.Download.Resume;

public sealed record ResumeMetadata(
    string TemporaryPath,
    bool Exists,
    long ExistingBytes);
