using Microsoft.EntityFrameworkCore;
using SmartInstaller.Core.Entities;

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

    public DbSet<SoftwareApplication> Applications =>
        Set<SoftwareApplication>();

    public DbSet<ApplicationVersion> ApplicationVersions =>
        Set<ApplicationVersion>();

    public DbSet<InstallerProfile> InstallerProfiles =>
        Set<InstallerProfile>();

    public DbSet<InstallerType> InstallerTypes =>
        Set<InstallerType>();

    public DbSet<Architecture> Architectures =>
        Set<Architecture>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}