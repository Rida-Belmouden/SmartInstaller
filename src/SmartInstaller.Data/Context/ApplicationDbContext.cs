using Microsoft.EntityFrameworkCore;
using SmartInstaller.Core.Common;
using SmartInstaller.Core.Entities.Catalog;
using SmartInstaller.Core.Entities.Installer;
using SmartInstaller.Data.Seed;

namespace SmartInstaller.Data.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<SoftwareApplication> Applications => Set<SoftwareApplication>();
    public DbSet<ApplicationVersion> ApplicationVersions => Set<ApplicationVersion>();
    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ApplicationTag> ApplicationTags => Set<ApplicationTag>();

    public DbSet<InstallerProfile> InstallerProfiles => Set<InstallerProfile>();
    public DbSet<InstallerType> InstallerTypes => Set<InstallerType>();
    public DbSet<Architecture> Architectures => Set<Architecture>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        DatabaseSeed.Seed(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditValues();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditValues();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditValues();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditValues();

        return base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    private void ApplyAuditValues()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.PublicId == Guid.Empty)
                {
                    entry.Entity.PublicId = Guid.NewGuid();
                }

                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = utcNow;
                }

                entry.Entity.UpdatedAt = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;

                entry.Property(entity => entity.CreatedAt)
                    .IsModified = false;

                entry.Property(entity => entity.PublicId)
                    .IsModified = false;
            }
        }
    }
}
