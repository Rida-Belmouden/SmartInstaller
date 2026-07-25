using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities.Catalog;

namespace SmartInstaller.Data.Configurations;

public class ApplicationVersionConfiguration
    : IEntityTypeConfiguration<ApplicationVersion>
{
    public void Configure(EntityTypeBuilder<ApplicationVersion> builder)
    {
        builder.ToTable("ApplicationVersions");

        builder.HasKey(version => version.Id);

        builder.Property(version => version.Version)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(version => version.ReleaseDate)
            .HasColumnType("date");

        builder.Property(version => version.IsLatest)
            .HasDefaultValue(false);

        builder.Property(version => version.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(version => version.SoftwareApplicationId);

        builder.HasIndex(version => new
        {
            version.SoftwareApplicationId,
            version.Version
        })
        .IsUnique();

        builder.HasMany(version => version.InstallerProfiles)
            .WithOne(profile => profile.ApplicationVersion)
            .HasForeignKey(profile => profile.ApplicationVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}