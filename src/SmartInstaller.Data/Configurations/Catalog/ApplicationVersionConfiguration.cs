using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities.Catalog;

namespace SmartInstaller.Data.Configurations.Catalog;

public sealed class ApplicationVersionConfiguration
    : IEntityTypeConfiguration<ApplicationVersion>
{
    public void Configure(EntityTypeBuilder<ApplicationVersion> builder)
    {
        builder.ToTable("ApplicationVersions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PublicId)
            .IsRequired();

        builder.HasIndex(x => x.PublicId)
            .IsUnique();

        builder.Property(x => x.Version)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ReleaseDate);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasOne(x => x.SoftwareApplication)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.SoftwareApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.InstallerProfiles)
            .WithOne(x => x.ApplicationVersion)
            .HasForeignKey(x => x.ApplicationVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
            {
                x.SoftwareApplicationId,
                x.Version
            })
            .IsUnique();
    }
}