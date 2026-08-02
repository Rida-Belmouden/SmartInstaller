using SmartInstaller.Agent.Core.Download.Cache;
using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public sealed class UpdateDownloadStateService(
    IAgentApiClient agentApiClient,
    IFileCacheService fileCacheService,
    IInstallerFileNameResolver fileNameResolver)
    : IUpdateDownloadStateService
{
    public async Task<UpdateDownloadState?> GetStateAsync(
        UpdateCheckItem update,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);

        if (!update.UpdateAvailable ||
            !update.InstallerProfileId.HasValue)
        {
            return null;
        }

        var manifest =
            await agentApiClient.GetInstallerManifestAsync(
                update.InstallerProfileId.Value,
                cancellationToken);

        if (!Uri.TryCreate(
                manifest.DownloadUrl,
                UriKind.Absolute,
                out var downloadUri))
        {
            return null;
        }

        var fileName =
            fileNameResolver.Resolve(
                manifest,
                downloadUri);

        fileCacheService
            .EnsureCacheDirectoryExists();

        var metadata =
            fileCacheService.GetResumeMetadata(
                fileName);

        var finalPath =
            fileCacheService.GetFinalPath(
                fileName);

        return new UpdateDownloadState(
            manifest,
            fileName,
            metadata.TemporaryPath,
            finalPath,
            File.Exists(finalPath),
            metadata.ExistingBytes,
            manifest.FileSizeBytes);
    }
}
