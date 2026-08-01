using SmartInstaller.Agent.Core.Models;
using SmartInstaller.Agent.Core.Services;

namespace SmartInstaller.Agent.Core.Installation.Verification;

public sealed class InstallationVerifier(
    IInstalledSoftwareScanner scanner,
    IApplicationNameNormalizer normalizer,
    IInstallationVerificationDelay verificationDelay)
    : IInstallationVerifier
{
    public async Task<InstallationVerificationResult> VerifyAsync(
        string applicationName,
        string expectedVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedVersion);

        await verificationDelay.WaitAsync(
            cancellationToken);

        var installedApplications =
            await scanner.ScanAsync(cancellationToken);

        var expectedName =
            normalizer.Normalize(applicationName);

        var matches = installedApplications
            .Where(application =>
                string.Equals(
                    application.NormalizedName,
                    expectedName,
                    StringComparison.Ordinal))
            .ToArray();

        if (matches.Length == 0)
        {
            return new InstallationVerificationResult(
                InstallationVerificationStatus.ApplicationNotFound,
                expectedVersion,
                null,
                null,
                $"'{applicationName}' was not found after installation.");
        }

        var detected = SelectBestMatch(
            matches,
            expectedVersion);

        if (string.IsNullOrWhiteSpace(detected.Version))
        {
            return new InstallationVerificationResult(
                InstallationVerificationStatus.VersionUnavailable,
                expectedVersion,
                null,
                detected,
                "The application was detected, but its installed version is unavailable.");
        }

        if (VersionsAreEquivalent(
                detected.Version,
                expectedVersion))
        {
            return new InstallationVerificationResult(
                InstallationVerificationStatus.Verified,
                expectedVersion,
                detected.Version,
                detected,
                null);
        }

        return new InstallationVerificationResult(
            InstallationVerificationStatus.VersionMismatch,
            expectedVersion,
            detected.Version,
            detected,
            $"Expected version {expectedVersion}, but detected {detected.Version}.");
    }

    private static InstalledApplication SelectBestMatch(
        IReadOnlyList<InstalledApplication> matches,
        string expectedVersion)
    {
        return matches.FirstOrDefault(application =>
                   !string.IsNullOrWhiteSpace(application.Version) &&
                   VersionsAreEquivalent(
                       application.Version,
                       expectedVersion))
               ?? matches.First();
    }

    private static bool VersionsAreEquivalent(
        string installedVersion,
        string expectedVersion)
    {
        var installed = installedVersion.Trim();
        var expected = expectedVersion.Trim();

        if (string.Equals(
                installed,
                expected,
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (Version.TryParse(installed, out var installedParsed) &&
            Version.TryParse(expected, out var expectedParsed))
        {
            return NormalizeVersion(installedParsed) ==
                   NormalizeVersion(expectedParsed);
        }

        return false;
    }

    private static Version NormalizeVersion(Version value)
    {
        return new Version(
            Math.Max(0, value.Major),
            Math.Max(0, value.Minor),
            Math.Max(0, value.Build),
            Math.Max(0, value.Revision));
    }
}
