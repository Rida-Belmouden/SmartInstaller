using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities;

namespace SmartInstaller.Data.Configurations;

public class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
{
    public void Configure(EntityTypeBuilder<Publisher> builder)
    {
        builder.ToTable("Publishers");

        builder.HasKey(publisher => publisher.Id);

        builder.Property(publisher => publisher.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(publisher => publisher.Website)
            .HasMaxLength(500);

        builder.Property(publisher => publisher.LogoUrl)
            .HasMaxLength(1000);

        builder.Property(publisher => publisher.IsVerified)
            .HasDefaultValue(false);

        builder.HasIndex(publisher => publisher.Name)
            .IsUnique();

        builder.HasMany(publisher => publisher.Applications)
            .WithOne(application => application.Publisher)
            .HasForeignKey(application => application.PublisherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}