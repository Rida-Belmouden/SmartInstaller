using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities;

namespace SmartInstaller.Data.Configurations;

public class SoftwareApplicationConfiguration
    : IEntityTypeConfiguration<SoftwareApplication>
{
    public void Configure(EntityTypeBuilder<SoftwareApplication> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(application => application.Id);

        builder.Property(application => application.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(application => application.Slug)
            .IsRequired()
            .HasMaxLength(170);

        builder.Property(application => application.Description)
            .HasMaxLength(2000);

        builder.Property(application => application.Website)
            .HasMaxLength(500);

        builder.Property(application => application.IconUrl)
            .HasMaxLength(1000);

        builder.Property(application => application.IsFeatured)
            .HasDefaultValue(false);

        builder.Property(application => application.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(application => application.Name);

        builder.HasIndex(application => application.Slug)
            .IsUnique();

        builder.HasIndex(application => application.CategoryId);

        builder.HasIndex(application => application.PublisherId);

        builder.HasMany(application => application.Versions)
            .WithOne(version => version.SoftwareApplication)
            .HasForeignKey(version => version.SoftwareApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}