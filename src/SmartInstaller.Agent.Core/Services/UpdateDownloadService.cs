using System.Text;
using SmartInstaller.Agent.Core.Download.Models;
using SmartInstaller.Agent.Core.Download.Services;
using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public sealed class UpdateDownloadService(
    IAgentApiClient agentApiClient,
    IDownloadManager downloadManager)
    : IUpdateDownloadService
{
    public async Task<UpdateDownloadResult> DownloadAsync(
        UpdateCheckItem update,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);

        if (!update.UpdateAvailable)
        {
            return new UpdateDownloadResult(
                update,
                null,
                DownloadResult.Failed(
                    "The selected application does not have an available update."));
        }

        if (!update.InstallerProfileId.HasValue)
        {
            return new UpdateDownloadResult(
                update,
                null,
                DownloadResult.Failed(
                    "No compatible installer profile is available for this update."));
        }

        var manifest = await agentApiClient.GetInstallerManifestAsync(
            update.InstallerProfileId.Value,
            cancellationToken);

        if (!Uri.TryCreate(
                manifest.DownloadUrl,
                UriKind.Absolute,
                out var downloadUri))
        {
            return new UpdateDownloadResult(
                update,
                manifest,
                DownloadResult.Failed(
                    "The installer manifest contains an invalid download URL."));
        }

        var request = new DownloadRequest(
            downloadUri,
            CreateFileName(manifest, downloadUri),
            manifest.Sha256,
            manifest.FileSizeBytes);

        var result = await downloadManager.DownloadAsync(
            request,
            progress,
            cancellationToken);

        return new UpdateDownloadResult(
            update,
            manifest,
            result);
    }

    private static string CreateFileName(
        InstallerManifest manifest,
        Uri downloadUri)
    {
        var sourceFileName = Path.GetFileName(
            Uri.UnescapeDataString(downloadUri.AbsolutePath));

        if (!string.IsNullOrWhiteSpace(sourceFileName) &&
            sourceFileName.IndexOfAny(
                Path.GetInvalidFileNameChars()) < 0)
        {
            return sourceFileName;
        }

        var extension = manifest.InstallerType.ToLowerInvariant() switch
        {
            "msi" => ".msi",
            "msix" => ".msix",
            "zip" => ".zip",
            _ => ".exe"
        };

        return $"{Sanitize(manifest.ApplicationName)}-{Sanitize(manifest.Version)}-{Sanitize(manifest.Architecture)}{extension}";
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var character in value.Trim())
        {
            builder.Append(
                invalid.Contains(character) || char.IsWhiteSpace(character)
                    ? '-'
                    : character);
        }

        return builder.ToString().Trim('-');
    }
}
