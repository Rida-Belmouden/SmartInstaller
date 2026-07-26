using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartInstaller.Core.Entities.Catalog;

namespace SmartInstaller.Data.Configurations.Catalog;

public sealed class ApplicationTagConfiguration
    : IEntityTypeConfiguration<ApplicationTag>
{
    public void Configure(EntityTypeBuilder<ApplicationTag> builder)
    {
        builder.ToTable("ApplicationTags");

        builder.HasKey(x => new
        {
            x.SoftwareApplicationId,
            x.TagId
        });

        builder.HasOne(x => x.SoftwareApplication)
            .WithMany(x => x.ApplicationTags)
            .HasForeignKey(x => x.SoftwareApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.ApplicationTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}