using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities.Installer;

namespace SmartInstaller.Data.Configurations.Installer;

public class InstallerTypeConfiguration
    : IEntityTypeConfiguration<InstallerType>
{
    public void Configure(EntityTypeBuilder<InstallerType> builder)
    {
        builder.ToTable("InstallerTypes");

        builder.HasKey(installerType => installerType.Id);

        builder.Property(installerType => installerType.Name)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(installerType => installerType.Description)
            .HasMaxLength(300);

        builder.Property(installerType => installerType.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(installerType => installerType.Name)
            .IsUnique();

        builder.HasMany(installerType => installerType.InstallerProfiles)
            .WithOne(profile => profile.InstallerType)
            .HasForeignKey(profile => profile.InstallerTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}