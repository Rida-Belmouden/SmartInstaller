using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartInstaller.Agent.Services;

public sealed partial class ApplicationNameNormalizer
    : IApplicationNameNormalizer
{
    public string Normalize(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) !=
                UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var normalized = builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();

        normalized = ArchitectureSuffixRegex().Replace(normalized, " ");
        normalized = VersionSuffixRegex().Replace(normalized, " ");
        normalized = NonAlphaNumericRegex().Replace(normalized, " ");
        normalized = WhitespaceRegex().Replace(normalized, " ").Trim();

        return normalized;
    }

    [GeneratedRegex(@"\b(?:x86|x64|32[ -]?bit|64[ -]?bit|arm64)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ArchitectureSuffixRegex();

    [GeneratedRegex(@"\b(?:version|ver|v)\s*\d+(?:[._-]\d+)*\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex VersionSuffixRegex();

    [GeneratedRegex(@"[^a-z0-9]+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"\s+",
        RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}
