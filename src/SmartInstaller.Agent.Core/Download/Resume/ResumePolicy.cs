namespace SmartInstaller.Agent.Core.Download.Resume;

public sealed class ResumePolicy : IResumePolicy
{
    public ResumeDecision Evaluate(
        ResumeMetadata metadata,
        long? expectedFileSizeBytes)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        if (!metadata.Exists || metadata.ExistingBytes <= 0)
        {
            return ResumeDecision.Fresh(
                "No usable partial download exists.");
        }

        if (expectedFileSizeBytes is < 0)
        {
            return ResumeDecision.Restart(
                metadata.ExistingBytes,
                "The expected file size is invalid.");
        }

        if (expectedFileSizeBytes.HasValue &&
            metadata.ExistingBytes >= expectedFileSizeBytes.Value)
        {
            return ResumeDecision.Restart(
                metadata.ExistingBytes,
                "The partial file is already equal to or larger than the expected file size.");
        }

        return ResumeDecision.Resume(
            metadata.ExistingBytes,
            "A partial download can be resumed.");
    }
}
