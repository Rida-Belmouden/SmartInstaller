namespace SmartInstaller.Agent.Core.Download.Resume;

public interface IResumePolicy
{
    ResumeDecision Evaluate(
        ResumeMetadata metadata,
        long? expectedFileSizeBytes);
}
