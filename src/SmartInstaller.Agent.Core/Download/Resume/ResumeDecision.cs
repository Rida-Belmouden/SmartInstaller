namespace SmartInstaller.Agent.Core.Download.Resume;

public sealed record ResumeDecision(
    ResumeMode Mode,
    long ExistingBytes,
    string Reason)
{
    public bool ShouldResume =>
        Mode == ResumeMode.ResumeFromPartialFile;

    public static ResumeDecision Fresh(string reason) =>
        new(ResumeMode.FreshDownload, 0, reason);

    public static ResumeDecision Resume(
        long existingBytes,
        string reason) =>
        new(
            ResumeMode.ResumeFromPartialFile,
            existingBytes,
            reason);

    public static ResumeDecision Restart(
        long existingBytes,
        string reason) =>
        new(
            ResumeMode.RestartDownload,
            existingBytes,
            reason);
}
