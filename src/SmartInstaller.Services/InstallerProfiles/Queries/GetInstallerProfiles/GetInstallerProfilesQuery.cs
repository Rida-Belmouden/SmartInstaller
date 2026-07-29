namespace SmartInstaller.Services.InstallerProfiles
    .Queries.GetInstallerProfiles;

public sealed record GetInstallerProfilesQuery(
    Guid? ApplicationVersionPublicId = null,
    Guid? InstallerTypePublicId = null,
    Guid? ArchitecturePublicId = null,
    bool? IsEnabled = null,
    bool IncludeInactive = false);
