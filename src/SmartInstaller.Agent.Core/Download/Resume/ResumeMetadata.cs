namespace SmartInstaller.Agent.Core.Download.Resume;

public sealed record ResumeMetadata(
    string TemporaryFilePath,
    bool Exists,
    long ExistingBytes);
