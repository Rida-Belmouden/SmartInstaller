using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities.Installer;

namespace SmartInstaller.Data.Configurations.Installer;

public sealed class ArchitectureConfiguration
    : IEntityTypeConfiguration<Architecture>
{
    public void Configure(
        EntityTypeBuilder<Architecture> builder)
    {
        builder.ToTable("Architectures");

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