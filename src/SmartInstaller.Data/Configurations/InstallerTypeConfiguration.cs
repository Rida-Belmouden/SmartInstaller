using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities;

namespace SmartInstaller.Data.Configurations;

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

        builder.HasData(
            new InstallerType
            {
                Id = 1,
                Name = "EXE",
                Description = "Windows executable installer",
                IsActive = true
            },
            new InstallerType
            {
                Id = 2,
                Name = "MSI",
                Description = "Microsoft Windows Installer package",
                IsActive = true
            },
            new InstallerType
            {
                Id = 3,
                Name = "MSIX",
                Description = "Modern Microsoft application package",
                IsActive = true
            },
            new InstallerType
            {
                Id = 4,
                Name = "APPX",
                Description = "Windows application package",
                IsActive = true
            },
            new InstallerType
            {
                Id = 5,
                Name = "ZIP",
                Description = "Portable compressed application",
                IsActive = true
            }
        );

        builder.HasMany(installerType => installerType.InstallerProfiles)
            .WithOne(profile => profile.InstallerType)
            .HasForeignKey(profile => profile.InstallerTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}