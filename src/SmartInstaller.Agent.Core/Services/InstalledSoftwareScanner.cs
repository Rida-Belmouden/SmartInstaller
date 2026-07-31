using System.Globalization;
using Microsoft.Win32;
using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public sealed class InstalledSoftwareScanner(
    IApplicationNameNormalizer nameNormalizer)
    : IInstalledSoftwareScanner
{
    private const string UninstallPath =
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    private static readonly RegistryLocation[] Locations =
    [
        new(RegistryHive.LocalMachine, RegistryView.Registry64),
        new(RegistryHive.LocalMachine, RegistryView.Registry32),
        new(RegistryHive.CurrentUser, RegistryView.Registry64),
        new(RegistryHive.CurrentUser, RegistryView.Registry32)
    ];

    public Task<IReadOnlyList<InstalledApplication>> ScanAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.Run(
            () => Scan(cancellationToken),
            cancellationToken);
    }

    private IReadOnlyList<InstalledApplication> Scan(
        CancellationToken cancellationToken)
    {
        var applications = new Dictionary<string, InstalledApplication>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var location in Locations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                ReadLocation(location, applications, cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                // Some registry branches can be inaccessible for standard users.
            }
            catch (System.Security.SecurityException)
            {
                // Continue scanning the remaining registry locations.
            }
        }

        return applications.Values
            .OrderBy(application => application.Name,
                StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(application => application.Version,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private void ReadLocation(
        RegistryLocation location,
        IDictionary<string, InstalledApplication> applications,
        CancellationToken cancellationToken)
    {
        using var baseKey = RegistryKey.OpenBaseKey(
            location.Hive,
            location.View);

        using var uninstallKey = baseKey.OpenSubKey(UninstallPath);

        if (uninstallKey is null)
        {
            return;
        }

        foreach (var subKeyName in uninstallKey.GetSubKeyNames())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var applicationKey = uninstallKey.OpenSubKey(subKeyName);

                if (applicationKey is null || ShouldIgnore(applicationKey))
                {
                    continue;
                }

                var displayName = ReadString(applicationKey, "DisplayName");

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    continue;
                }

                var normalizedName = nameNormalizer.Normalize(displayName);

                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    continue;
                }

                var application = new InstalledApplication(
                    Name: displayName.Trim(),
                    Version: ReadString(applicationKey, "DisplayVersion"),
                    Publisher: ReadString(applicationKey, "Publisher"),
                    InstallLocation: ReadString(applicationKey, "InstallLocation"),
                    UninstallString: ReadString(applicationKey, "UninstallString"),
                    InstallDate: ParseInstallDate(
                        ReadString(applicationKey, "InstallDate")),
                    NormalizedName: normalizedName,
                    RegistryPath: $@"{UninstallPath}\{subKeyName}",
                    RegistryHive: location.Hive.ToString(),
                    RegistryView: location.View.ToString());

                var identity = BuildIdentity(application);

                if (!applications.TryGetValue(identity, out var existing) ||
                    IsMoreComplete(application, existing))
                {
                    applications[identity] = application;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore only this product entry and continue the scan.
            }
            catch (System.Security.SecurityException)
            {
                // Ignore only this product entry and continue the scan.
            }
            catch (IOException)
            {
                // An entry can disappear while software is being installed/uninstalled.
            }
        }
    }

    private static bool ShouldIgnore(RegistryKey key)
    {
        return ReadInt32(key, "SystemComponent") == 1 ||
               ReadInt32(key, "NoDisplay") == 1 ||
               !string.IsNullOrWhiteSpace(ReadString(key, "ParentKeyName"));
    }

    private static string BuildIdentity(InstalledApplication application) =>
        string.Join('|',
            application.NormalizedName,
            application.Version?.Trim().ToLowerInvariant() ?? string.Empty,
            application.Publisher?.Trim().ToLowerInvariant() ?? string.Empty);

    private static bool IsMoreComplete(
        InstalledApplication candidate,
        InstalledApplication existing)
    {
        return CompletenessScore(candidate) > CompletenessScore(existing);
    }

    private static int CompletenessScore(InstalledApplication application)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(application.Version)) score++;
        if (!string.IsNullOrWhiteSpace(application.Publisher)) score++;
        if (!string.IsNullOrWhiteSpace(application.InstallLocation)) score++;
        if (!string.IsNullOrWhiteSpace(application.UninstallString)) score++;
        if (application.InstallDate.HasValue) score++;

        return score;
    }

    private static string? ReadString(RegistryKey key, string valueName)
    {
        return key.GetValue(valueName) switch
        {
            string value when !string.IsNullOrWhiteSpace(value) => value.Trim(),
            _ => null
        };
    }

    private static int? ReadInt32(RegistryKey key, string valueName)
    {
        return key.GetValue(valueName) switch
        {
            int value => value,
            string value when int.TryParse(value, out var parsed) => parsed,
            _ => null
        };
    }

    private static DateOnly? ParseInstallDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var formats = new[] { "yyyyMMdd", "yyyy-MM-dd", "MM/dd/yyyy" };

        return DateOnly.TryParseExact(
            value,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date)
                ? date
                : null;
    }

    private sealed record RegistryLocation(
        RegistryHive Hive,
        RegistryView View);
}
