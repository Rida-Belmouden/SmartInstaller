using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities;

namespace SmartInstaller.Data.Configurations;

public class ArchitectureConfiguration
    : IEntityTypeConfiguration<Architecture>
{
    public void Configure(EntityTypeBuilder<Architecture> builder)
    {
        builder.ToTable("Architectures");

        builder.HasKey(architecture => architecture.Id);

        builder.Property(architecture => architecture.Name)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(architecture => architecture.Description)
            .HasMaxLength(300);

        builder.Property(architecture => architecture.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(architecture => architecture.Name)
            .IsUnique();

        builder.HasData(
            new Architecture
            {
                Id = 1,
                Name = "x86",
                Description = "32-bit Windows architecture",
                IsActive = true
            },
            new Architecture
            {
                Id = 2,
                Name = "x64",
                Description = "64-bit Windows architecture",
                IsActive = true
            },
            new Architecture
            {
                Id = 3,
                Name = "ARM64",
                Description = "64-bit ARM architecture",
                IsActive = true
            },
            new Architecture
            {
                Id = 4,
                Name = "Any",
                Description = "Architecture-independent installer",
                IsActive = true
            }
        );

        builder.HasMany(architecture => architecture.InstallerProfiles)
            .WithOne(profile => profile.Architecture)
            .HasForeignKey(profile => profile.ArchitectureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}