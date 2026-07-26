using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities.Installer;

namespace SmartInstaller.Data.Configurations.Installer;

public sealed class ArchitectureConfiguration
    : IEntityTypeConfiguration<Architecture>
{
    public void Configure(EntityTypeBuilder<Architecture> builder)
    {
        builder.ToTable("Architectures");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PublicId)
            .IsRequired();

        builder.HasIndex(x => x.PublicId)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

    }
}