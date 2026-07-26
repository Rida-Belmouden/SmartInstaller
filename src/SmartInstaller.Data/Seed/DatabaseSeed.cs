using Microsoft.EntityFrameworkCore;
using SmartInstaller.Core.Entities.Catalog;
using SmartInstaller.Core.Entities.Installer;

namespace SmartInstaller.Data.Seed;

public static class DatabaseSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedPlatforms(modelBuilder);
        SeedArchitectures(modelBuilder);
        SeedInstallerTypes(modelBuilder);
        SeedCategories(modelBuilder);
    }

    private static void SeedPlatforms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Platform>().HasData(
            new Platform
            {
                Id = 1,
                PublicId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "Windows",
                Slug = "windows",
                Description = "Microsoft Windows operating system",
                CreatedAt = new DateTime(2026, 7, 25, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            }
        );
    }

    private static void SeedArchitectures(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Architecture>().HasData(
            new Architecture
            {
                Id = 1,
                PublicId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                Name = "x86",
                CreatedAt = new DateTime(2026, 7, 25, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            },
            new Architecture
            {
                Id = 2,
                PublicId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                Name = "x64",
                CreatedAt = new DateTime(2026, 7, 25, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            },
            new Architecture
            {
                Id = 3,
                PublicId = Guid.Parse("20000000-0000-0000-0000-000000000003"),
                Name = "ARM64",
                CreatedAt = new DateTime(2026, 7, 25, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            }
        );
    }

    private static void SeedInstallerTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InstallerType>().HasData(
            CreateInstallerType(
                1,
                "EXE",
                "30000000-0000-0000-0000-000000000001"),

            CreateInstallerType(
                2,
                "MSI",
                "30000000-0000-0000-0000-000000000002"),

            CreateInstallerType(
                3,
                "MSIX",
                "30000000-0000-0000-0000-000000000003"),

            CreateInstallerType(
                4,
                "ZIP",
                "30000000-0000-0000-0000-000000000004")
        );
    }

    private static InstallerType CreateInstallerType(
        int id,
        string name,
        string publicId)
    {
        return new InstallerType
        {
            Id = id,
            PublicId = Guid.Parse(publicId),
            Name = name,
            CreatedAt = new DateTime(
                2026, 7, 25, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        };
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            CreateCategory(1, "Browsers", "browsers",
                "40000000-0000-0000-0000-000000000001"),

            CreateCategory(2, "Messaging", "messaging",
                "40000000-0000-0000-0000-000000000002"),

            CreateCategory(3, "Media", "media",
                "40000000-0000-0000-0000-000000000003"),

            CreateCategory(4, "Development", "development",
                "40000000-0000-0000-0000-000000000004"),

            CreateCategory(5, "Utilities", "utilities",
                "40000000-0000-0000-0000-000000000005"),

            CreateCategory(6, "Security", "security",
                "40000000-0000-0000-0000-000000000006"),

            CreateCategory(7, "Compression", "compression",
                "40000000-0000-0000-0000-000000000007"),

            CreateCategory(8, "Cloud Storage", "cloud-storage",
                "40000000-0000-0000-0000-000000000008")
        );
    }

    private static Category CreateCategory(
        int id,
        string name,
        string slug,
        string publicId)
    {
        return new Category
        {
            Id = id,
            PublicId = Guid.Parse(publicId),
            Name = name,
            Slug = slug,
            CreatedAt = new DateTime(
                2026, 7, 25, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true
        };
    }
}