using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities;

namespace SmartInstaller.Data.Configurations;

public class InstallerProfileConfiguration
    : IEntityTypeConfiguration<InstallerProfile>
{
    public void Configure(EntityTypeBuilder<InstallerProfile> builder)
    {
        builder.ToTable("InstallerProfiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.DownloadUrl)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(profile => profile.Sha256)
            .HasMaxLength(64)
            .IsFixedLength();

        builder.Property(profile => profile.SilentInstallArguments)
            .HasMaxLength(1000);

        builder.Property(profile => profile.SilentUninstallArguments)
            .HasMaxLength(1000);

        builder.Property(profile => profile.RequiresAdministrator)
            .HasDefaultValue(true);

        builder.Property(profile => profile.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(profile => profile.ApplicationVersionId);

        builder.HasIndex(profile => profile.InstallerTypeId);

        builder.HasIndex(profile => profile.ArchitectureId);

        builder.HasIndex(profile => new
        {
            profile.ApplicationVersionId,
            profile.InstallerTypeId,
            profile.ArchitectureId
        })
        .IsUnique();

        builder.HasOne(profile => profile.ApplicationVersion)
            .WithMany(version => version.InstallerProfiles)
            .HasForeignKey(profile => profile.ApplicationVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(profile => profile.InstallerType)
            .WithMany(type => type.InstallerProfiles)
            .HasForeignKey(profile => profile.InstallerTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(profile => profile.Architecture)
            .WithMany(architecture => architecture.InstallerProfiles)
            .HasForeignKey(profile => profile.ArchitectureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}