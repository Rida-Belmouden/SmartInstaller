using Microsoft.EntityFrameworkCore;
using SmartInstaller.Data.Context;
using SmartInstaller.Services.InstallerProfiles.Common;

namespace SmartInstaller.Services.InstallerProfiles
    .Queries.GetInstallerProfileById;

public sealed class GetInstallerProfileByIdHandler(
    ApplicationDbContext dbContext)
    : IGetInstallerProfileByIdHandler
{
    public async Task<GetInstallerProfileByIdResult> HandleAsync(
        GetInstallerProfileByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.InstallerProfiles
            .AsNoTracking()
            .Include(x => x.ApplicationVersion)
            .Include(x => x.InstallerType)
            .Include(x => x.Architecture)
            .FirstOrDefaultAsync(
                x => x.PublicId == query.PublicId && x.IsActive,
                cancellationToken);

        if (profile is null)
        {
            return new GetInstallerProfileByIdResult(
                GetInstallerProfileByIdStatus.NotFound);
        }

        return new GetInstallerProfileByIdResult(
            GetInstallerProfileByIdStatus.Success,
            InstallerProfileMapper.Map(profile));
    }
}
