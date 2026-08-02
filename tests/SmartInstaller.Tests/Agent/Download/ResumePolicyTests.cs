using SmartInstaller.Agent.Core.Download.Resume;

namespace SmartInstaller.Tests.Agent.Download;

public sealed class ResumePolicyTests
{
    private readonly ResumePolicy _policy = new();

    [Fact]
    public void Evaluate_WhenPartialFileDoesNotExist_StartsFresh()
    {
        var decision = _policy.Evaluate(
            new ResumeMetadata("setup.download", false, 0),
            1000);

        Assert.Equal(
            ResumeMode.FreshDownload,
            decision.Mode);

        Assert.False(decision.ShouldResume);
    }

    [Fact]
    public void Evaluate_WhenPartialFileIsEmpty_StartsFresh()
    {
        var decision = _policy.Evaluate(
            new ResumeMetadata("setup.download", true, 0),
            1000);

        Assert.Equal(
            ResumeMode.FreshDownload,
            decision.Mode);
    }

    [Fact]
    public void Evaluate_WhenPartialFileIsSmaller_Resumes()
    {
        var decision = _policy.Evaluate(
            new ResumeMetadata("setup.download", true, 400),
            1000);

        Assert.Equal(
            ResumeMode.ResumeFromPartialFile,
            decision.Mode);

        Assert.True(decision.ShouldResume);
        Assert.Equal(400, decision.ExistingBytes);
    }

    [Fact]
    public void Evaluate_WhenExpectedSizeIsUnknown_Resumes()
    {
        var decision = _policy.Evaluate(
            new ResumeMetadata("setup.download", true, 400),
            null);

        Assert.True(decision.ShouldResume);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(1200)]
    public void Evaluate_WhenPartialFileIsNotSmaller_Restarts(
        long existingBytes)
    {
        var decision = _policy.Evaluate(
            new ResumeMetadata(
                "setup.download",
                true,
                existingBytes),
            1000);

        Assert.Equal(
            ResumeMode.RestartDownload,
            decision.Mode);
    }
}
