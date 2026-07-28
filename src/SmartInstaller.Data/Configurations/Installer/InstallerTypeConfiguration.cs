using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities.Installer;

namespace SmartInstaller.Data.Configurations.Installer;

public sealed class InstallerTypeConfiguration
    : IEntityTypeConfiguration<InstallerType>
{
    public void Configure(
        EntityTypeBuilder<InstallerType> builder)
    {
        builder.ToTable("InstallerTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(250);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}