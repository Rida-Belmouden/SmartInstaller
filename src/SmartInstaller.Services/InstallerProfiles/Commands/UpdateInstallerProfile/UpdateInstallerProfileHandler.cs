using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.InstallerProfiles.Common;

namespace SmartInstaller.Services.InstallerProfiles
    .Commands.UpdateInstallerProfile;

public sealed class UpdateInstallerProfileHandler(
    ApplicationDbContext dbContext)
    : IUpdateInstallerProfileHandler
{
    public async Task<UpdateInstallerProfileResult> HandleAsync(
        UpdateInstallerProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var downloadUrl = command.DownloadUrl.Trim();
        var sha256 = InstallerProfileValidation.NormalizeOptional(command.Sha256);

        if (!InstallerProfileValidation.IsValidDownloadUrl(downloadUrl))
        {
            return new UpdateInstallerProfileResult(
                UpdateInstallerProfileStatus.InvalidDownloadUrl);
        }

        if (!InstallerProfileValidation.IsValidSha256(sha256))
        {
            return new UpdateInstallerProfileResult(
                UpdateInstallerProfileStatus.InvalidSha256);
        }

        if (command.FileSizeBytes is < 0)
        {
            return new UpdateInstallerProfileResult(
                UpdateInstallerProfileStatus.InvalidFileSize);
        }

        var profile = await dbContext.InstallerProfiles
            .Include(x => x.ApplicationVersion)
            .Include(x => x.InstallerType)
            .Include(x => x.Architecture)
            .FirstOrDefaultAsync(
                x => x.PublicId == command.PublicId && x.IsActive,
                cancellationToken);

        if (profile is null)
        {
            return new UpdateInstallerProfileResult(
                UpdateInstallerProfileStatus.InstallerProfileNotFound);
        }

        var installerType = await dbContext.InstallerTypes
            .FirstOrDefaultAsync(
                x => x.PublicId == command.InstallerTypePublicId &&
                     x.IsActive,
                cancellationToken);

        if (installerType is null)
        {
            return new UpdateInstallerProfileResult(
                UpdateInstallerProfileStatus.InstallerTypeNotFound);
        }

        var architecture = await dbContext.Architectures
            .FirstOrDefaultAsync(
                x => x.PublicId == command.ArchitecturePublicId &&
                     x.IsActive,
                cancellationToken);

        if (architecture is null)
        {
            return new UpdateInstallerProfileResult(
                UpdateInstallerProfileStatus.ArchitectureNotFound);
        }

        var duplicateExists = await dbContext.InstallerProfiles.AnyAsync(
            x =>
                x.Id != profile.Id &&
                x.ApplicationVersionId == profile.ApplicationVersionId &&
                x.InstallerTypeId == installerType.Id &&
                x.ArchitectureId == architecture.Id,
            cancellationToken);

        if (duplicateExists)
        {
            return new UpdateInstallerProfileResult(
                UpdateInstallerProfileStatus.DuplicateInstallerProfile);
        }

        profile.InstallerTypeId = installerType.Id;
        profile.ArchitectureId = architecture.Id;
        profile.DownloadUrl = downloadUrl;
        profile.Sha256 = sha256;
        profile.FileSizeBytes = command.FileSizeBytes;
        profile.SilentInstallArguments =
            InstallerProfileValidation.NormalizeOptional(
                command.SilentInstallArguments);
        profile.SilentUninstallArguments =
            InstallerProfileValidation.NormalizeOptional(
                command.SilentUninstallArguments);
        profile.RequiresAdministrator = command.RequiresAdministrator;
        profile.IsPortable = command.IsPortable;
        profile.IsEnabled = command.IsEnabled;

        await dbContext.SaveChangesAsync(cancellationToken);

        profile.InstallerType = installerType;
        profile.Architecture = architecture;

        return new UpdateInstallerProfileResult(
            UpdateInstallerProfileStatus.Success,
            InstallerProfileMapper.Map(profile));
    }
}
