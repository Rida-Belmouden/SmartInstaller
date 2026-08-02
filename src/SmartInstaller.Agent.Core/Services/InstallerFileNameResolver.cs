using System.Text;
using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public sealed class InstallerFileNameResolver
    : IInstallerFileNameResolver
{
    public string Resolve(
        InstallerManifest manifest,
        Uri downloadUri)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(downloadUri);

        var sourceFileName = Path.GetFileName(
            Uri.UnescapeDataString(
                downloadUri.AbsolutePath));

        if (!string.IsNullOrWhiteSpace(sourceFileName) &&
            sourceFileName.IndexOfAny(
                Path.GetInvalidFileNameChars()) < 0 &&
            HasSupportedExtension(sourceFileName))
        {
            return sourceFileName;
        }


        var extension =
            manifest.InstallerType
                .ToLowerInvariant() switch
            {
                "msi" => ".msi",
                "msix" => ".msix",
                "zip" => ".zip",
                _ => ".exe"
            };

        return
            $"{Sanitize(manifest.ApplicationName)}-" +
            $"{Sanitize(manifest.Version)}-" +
            $"{Sanitize(manifest.Architecture)}" +
            extension;
    }

    private static bool HasSupportedExtension(
        string fileName)
    {
        var extension =
            Path.GetExtension(fileName);

        return extension.Equals(
                   ".exe",
                   StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(
                   ".msi",
                   StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(
                   ".msix",
                   StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(
                   ".zip",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string Sanitize(string value)
    {
        var invalid =
            Path.GetInvalidFileNameChars();

        var builder =
            new StringBuilder(value.Length);

        foreach (var character in value.Trim())
        {
            builder.Append(
                invalid.Contains(character) ||
                char.IsWhiteSpace(character)
                    ? '-'
                    : character);
        }

        return builder
            .ToString()
            .Trim('-');
    }
}
