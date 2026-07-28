using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities.Installer;

namespace SmartInstaller.Data.Configurations.Installer;

public sealed class InstallerProfileConfiguration
    : IEntityTypeConfiguration<InstallerProfile>
{
    public void Configure(
        EntityTypeBuilder<InstallerProfile> builder)
    {
        builder.ToTable("InstallerProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DownloadUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.Sha256)
            .HasMaxLength(64);

        builder.Property(x => x.SilentInstallArguments)
            .HasMaxLength(1000);

        builder.Property(x => x.SilentUninstallArguments)
            .HasMaxLength(1000);

        builder.Property(x => x.RequiresAdministrator)
            .HasDefaultValue(true);

        builder.Property(x => x.IsPortable)
            .HasDefaultValue(false);

        builder.Property(x => x.IsEnabled)
            .HasDefaultValue(true);

        builder.HasOne(x => x.ApplicationVersion)
            .WithMany(x => x.InstallerProfiles)
            .HasForeignKey(x => x.ApplicationVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.InstallerType)
            .WithMany(x => x.InstallerProfiles)
            .HasForeignKey(x => x.InstallerTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Architecture)
            .WithMany(x => x.InstallerProfiles)
            .HasForeignKey(x => x.ArchitectureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.ApplicationVersionId,
            x.InstallerTypeId,
            x.ArchitectureId
        })
            .IsUnique();
    }
}