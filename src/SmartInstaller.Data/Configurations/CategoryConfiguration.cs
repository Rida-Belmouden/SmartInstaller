using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities;

namespace SmartInstaller.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(category => category.Slug)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(category => category.Description)
            .HasMaxLength(500);

        builder.Property(category => category.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(category => category.Name)
            .IsUnique();

        builder.HasIndex(category => category.Slug)
            .IsUnique();

        builder.HasMany(category => category.Applications)
            .WithOne(application => application.Category)
            .HasForeignKey(application => application.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}