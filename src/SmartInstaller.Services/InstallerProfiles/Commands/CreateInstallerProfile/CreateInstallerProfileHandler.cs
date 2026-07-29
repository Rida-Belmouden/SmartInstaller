using Microsoft.EntityFrameworkCore;
using SmartInstaller.Core.Entities.Installer;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.InstallerProfiles.DTOs;

namespace SmartInstaller.Services.InstallerProfiles
    .Commands.CreateInstallerProfile;

public sealed class CreateInstallerProfileHandler(
    ApplicationDbContext dbContext)
    : ICreateInstallerProfileHandler
{
    public async Task<CreateInstallerProfileResult> HandleAsync(
        CreateInstallerProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedDownloadUrl = command.DownloadUrl.Trim();
        var normalizedSha256 = NormalizeOptional(command.Sha256);
        var normalizedInstallArguments =
            NormalizeOptional(command.SilentInstallArguments);
        var normalizedUninstallArguments =
            NormalizeOptional(command.SilentUninstallArguments);

        if (!IsValidDownloadUrl(normalizedDownloadUrl))
        {
            return new CreateInstallerProfileResult(
                CreateInstallerProfileStatus.InvalidDownloadUrl);
        }

        if (!IsValidSha256(normalizedSha256))
        {
            return new CreateInstallerProfileResult(
                CreateInstallerProfileStatus.InvalidSha256);
        }

        if (command.FileSizeBytes is < 0)
        {
            return new CreateInstallerProfileResult(
                CreateInstallerProfileStatus.InvalidFileSize);
        }

        var applicationVersion = await dbContext.ApplicationVersions
            .FirstOrDefaultAsync(
                version =>
                    version.PublicId ==
                        command.ApplicationVersionPublicId &&
                    version.IsActive,
                cancellationToken);

        if (applicationVersion is null)
        {
            return new CreateInstallerProfileResult(
                CreateInstallerProfileStatus.ApplicationVersionNotFound);
        }

        var installerType = await dbContext.InstallerTypes
            .FirstOrDefaultAsync(
                type =>
                    type.PublicId == command.InstallerTypePublicId &&
                    type.IsActive,
                cancellationToken);

        if (installerType is null)
        {
            return new CreateInstallerProfileResult(
                CreateInstallerProfileStatus.InstallerTypeNotFound);
        }

        var architecture = await dbContext.Architectures
            .FirstOrDefaultAsync(
                item =>
                    item.PublicId == command.ArchitecturePublicId &&
                    item.IsActive,
                cancellationToken);

        if (architecture is null)
        {
            return new CreateInstallerProfileResult(
                CreateInstallerProfileStatus.ArchitectureNotFound);
        }

        var duplicateExists = await dbContext.InstallerProfiles
            .AnyAsync(
                profile =>
                    profile.ApplicationVersionId ==
                        applicationVersion.Id &&
                    profile.InstallerTypeId == installerType.Id &&
                    profile.ArchitectureId == architecture.Id,
                cancellationToken);

        if (duplicateExists)
        {
            return new CreateInstallerProfileResult(
                CreateInstallerProfileStatus.DuplicateInstallerProfile);
        }

        var installerProfile = new InstallerProfile
        {
            ApplicationVersionId = applicationVersion.Id,
            InstallerTypeId = installerType.Id,
            ArchitectureId = architecture.Id,
            DownloadUrl = normalizedDownloadUrl,
            Sha256 = normalizedSha256,
            FileSizeBytes = command.FileSizeBytes,
            SilentInstallArguments = normalizedInstallArguments,
            SilentUninstallArguments = normalizedUninstallArguments,
            RequiresAdministrator = command.RequiresAdministrator,
            IsPortable = command.IsPortable,
            IsEnabled = command.IsEnabled,
            IsActive = true
        };

        dbContext.InstallerProfiles.Add(installerProfile);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new InstallerProfileDto(
            installerProfile.PublicId,
            applicationVersion.PublicId,
            installerType.PublicId,
            installerType.Name,
            architecture.PublicId,
            architecture.Name,
            installerProfile.DownloadUrl,
            installerProfile.Sha256,
            installerProfile.FileSizeBytes,
            installerProfile.SilentInstallArguments,
            installerProfile.SilentUninstallArguments,
            installerProfile.RequiresAdministrator,
            installerProfile.IsPortable,
            installerProfile.IsEnabled,
            installerProfile.IsActive);

        return new CreateInstallerProfileResult(
            CreateInstallerProfileStatus.Success,
            dto);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static bool IsValidDownloadUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp ||
               uri.Scheme == Uri.UriSchemeHttps;
    }

    private static bool IsValidSha256(string? value)
    {
        if (value is null)
        {
            return true;
        }

        return value.Length == 64 &&
               value.All(Uri.IsHexDigit);
    }
}
