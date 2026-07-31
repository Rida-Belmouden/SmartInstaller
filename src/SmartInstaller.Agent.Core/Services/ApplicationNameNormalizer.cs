using System.Text;
using System.Text.RegularExpressions;

namespace SmartInstaller.Agent.Core.Services;

public sealed partial class ApplicationNameNormalizer
    : IApplicationNameNormalizer
{
    private static readonly string[] IgnoredTokens =
    [
        "64 bit",
        "32 bit",
        "x64",
        "x86",
        "amd64",
        "arm64",
        "desktop",
        "application",
        "app"
    ];

    public string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value
            .Trim()
            .ToLowerInvariant();

        normalized = RemoveParenthesizedContent(normalized);

        normalized = TrailingVersionRegex()
            .Replace(normalized, string.Empty);

        foreach (var token in IgnoredTokens)
        {
            normalized = normalized.Replace(
                token,
                " ",
                StringComparison.Ordinal);
        }

        var builder = new StringBuilder(normalized.Length);
        var previousWasSpace = false;

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSpace = false;
            }
            else if (!previousWasSpace)
            {
                builder.Append(' ');
                previousWasSpace = true;
            }
        }

        return string.Join(
            ' ',
            builder
                .ToString()
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries));
    }

    private static string RemoveParenthesizedContent(
        string value)
    {
        var normalized = value;

        while (normalized.Contains('('))
        {
            var startIndex = normalized.IndexOf('(');
            var endIndex = normalized.IndexOf(
                ')',
                startIndex);

            if (endIndex == -1)
            {
                break;
            }

            normalized = normalized.Remove(
                startIndex,
                endIndex - startIndex + 1);
        }

        return normalized;
    }

    [GeneratedRegex(
        @"\s+\d+(?:\.\d+)+(?:[-+._a-z0-9]*)?\s*$",
        RegexOptions.IgnoreCase)]
    private static partial Regex TrailingVersionRegex();
}