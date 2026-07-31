using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Tests.Agent;

public sealed class ApplicationNameNormalizerTests
{
    private readonly ApplicationNameNormalizer _normalizer = new();

    [Theory]
    [InlineData("Google Chrome (64-bit)", "google chrome")]
    [InlineData("Visual Studio Code x64", "visual studio code")]
    [InlineData("  7-Zip  ", "7 zip")]
    public void Normalize_ReturnsStableComparableName(string value, string expected)
    {
        Assert.Equal(expected, _normalizer.Normalize(value));
    }
}