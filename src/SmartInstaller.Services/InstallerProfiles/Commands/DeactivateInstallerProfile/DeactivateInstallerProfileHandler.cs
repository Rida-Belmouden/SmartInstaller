using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;

namespace SmartInstaller.Services.InstallerProfiles
    .Commands.DeactivateInstallerProfile;

public sealed class DeactivateInstallerProfileHandler(
    ApplicationDbContext dbContext)
    : IDeactivateInstallerProfileHandler
{
    public async Task<DeactivateInstallerProfileResult> HandleAsync(
        DeactivateInstallerProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.InstallerProfiles
            .FirstOrDefaultAsync(
                x => x.PublicId == command.PublicId,
                cancellationToken);

        if (profile is null)
        {
            return new DeactivateInstallerProfileResult(
                DeactivateInstallerProfileStatus.NotFound);
        }

        if (!profile.IsActive)
        {
            return new DeactivateInstallerProfileResult(
                DeactivateInstallerProfileStatus.AlreadyInactive);
        }

        profile.IsActive = false;
        profile.IsEnabled = false;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeactivateInstallerProfileResult(
            DeactivateInstallerProfileStatus.Success);
    }
}
