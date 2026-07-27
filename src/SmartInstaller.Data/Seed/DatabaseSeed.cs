using Microsoft.EntityFrameworkCore;
using SmartInstaller.Core.Entities.Catalog;
using SmartInstaller.Core.Entities.Installer;

namespace SmartInstaller.Data.Seed;

public static class DatabaseSeed
{
    private static readonly DateTime SeedDate =
        new(2026, 7, 25, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedPlatforms(modelBuilder);
        SeedArchitectures(modelBuilder);
        SeedInstallerTypes(modelBuilder);
        SeedCategories(modelBuilder);
        SeedPublishers(modelBuilder);
        SeedTags(modelBuilder);
        SeedApplications(modelBuilder);
        SeedApplicationTags(modelBuilder);
    }

    private static void SeedPlatforms(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Platform>().HasData(
            new Platform
            {
                Id = 1,
                PublicId = Guid.Parse(
                    "10000000-0000-0000-0000-000000000001"),
                Name = "Windows",
                Slug = "windows",
                Description = "Microsoft Windows operating system",
                CreatedAt = SeedDate,
                IsActive = true
            });
    }

    private static void SeedArchitectures(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Architecture>().HasData(
            CreateArchitecture(
                1,
                "x86",
                "20000000-0000-0000-0000-000000000001"),

            CreateArchitecture(
                2,
                "x64",
                "20000000-0000-0000-0000-000000000002"),

            CreateArchitecture(
                3,
                "ARM64",
                "20000000-0000-0000-0000-000000000003"));
    }

    private static Architecture CreateArchitecture(
        int id,
        string name,
        string publicId)
    {
        return new Architecture
        {
            Id = id,
            PublicId = Guid.Parse(publicId),
            Name = name,
            CreatedAt = SeedDate,
            IsActive = true
        };
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
                "30000000-0000-0000-0000-000000000004"));
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
            CreatedAt = SeedDate,
            IsActive = true
        };
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            CreateCategory(
                1,
                "Browsers",
                "browsers",
                "40000000-0000-0000-0000-000000000001"),

            CreateCategory(
                2,
                "Messaging",
                "messaging",
                "40000000-0000-0000-0000-000000000002"),

            CreateCategory(
                3,
                "Media",
                "media",
                "40000000-0000-0000-0000-000000000003"),

            CreateCategory(
                4,
                "Development",
                "development",
                "40000000-0000-0000-0000-000000000004"),

            CreateCategory(
                5,
                "Utilities",
                "utilities",
                "40000000-0000-0000-0000-000000000005"),

            CreateCategory(
                6,
                "Security",
                "security",
                "40000000-0000-0000-0000-000000000006"),

            CreateCategory(
                7,
                "Compression",
                "compression",
                "40000000-0000-0000-0000-000000000007"),

            CreateCategory(
                8,
                "Cloud Storage",
                "cloud-storage",
                "40000000-0000-0000-0000-000000000008"));
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
            CreatedAt = SeedDate,
            IsActive = true
        };
    }

    private static void SeedPublishers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Publisher>().HasData(
            CreatePublisher(
                1,
                "Igor Pavlov",
                "https://www.7-zip.org",
                "50000000-0000-0000-0000-000000000001"),

            CreatePublisher(
                2,
                "VideoLAN",
                "https://www.videolan.org",
                "50000000-0000-0000-0000-000000000002"),

            CreatePublisher(
                3,
                "Mozilla",
                "https://www.mozilla.org",
                "50000000-0000-0000-0000-000000000003"),

            CreatePublisher(
                4,
                "Google",
                "https://www.google.com",
                "50000000-0000-0000-0000-000000000004"),

            CreatePublisher(
                5,
                "Microsoft",
                "https://www.microsoft.com",
                "50000000-0000-0000-0000-000000000005"));
    }

    private static Publisher CreatePublisher(
        int id,
        string name,
        string website,
        string publicId)
    {
        return new Publisher
        {
            Id = id,
            PublicId = Guid.Parse(publicId),
            Name = name,
            Website = website,
            IsVerified = true,
            CreatedAt = SeedDate,
            IsActive = true
        };
    }

    private static void SeedTags(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>().HasData(
            CreateTag(
                1,
                "Browser",
                "browser",
                "Web browsing software",
                "60000000-0000-0000-0000-000000000001"),

            CreateTag(
                2,
                "Media Player",
                "media-player",
                "Audio and video playback software",
                "60000000-0000-0000-0000-000000000002"),

            CreateTag(
                3,
                "Developer Tool",
                "developer-tool",
                "Software development tools",
                "60000000-0000-0000-0000-000000000003"),

            CreateTag(
                4,
                "Code Editor",
                "code-editor",
                "Source code editing software",
                "60000000-0000-0000-0000-000000000004"),

            CreateTag(
                5,
                "Open Source",
                "open-source",
                "Open-source software",
                "60000000-0000-0000-0000-000000000005"),

            CreateTag(
                6,
                "Compression",
                "compression",
                "File compression and extraction software",
                "60000000-0000-0000-0000-000000000006"),

            CreateTag(
                7,
                "Utility",
                "utility",
                "General system utility",
                "60000000-0000-0000-0000-000000000007"));
    }

    private static Tag CreateTag(
        int id,
        string name,
        string slug,
        string description,
        string publicId)
    {
        return new Tag
        {
            Id = id,
            PublicId = Guid.Parse(publicId),
            Name = name,
            Slug = slug,
            Description = description,
            CreatedAt = SeedDate,
            IsActive = true
        };
    }

    private static void SeedApplications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SoftwareApplication>().HasData(
            CreateApplication(
                id: 1,
                categoryId: 7,
                publisherId: 1,
                name: "7-Zip",
                slug: "7-zip",
                description:
                    "A file archiver with a high compression ratio.",
                website: "https://www.7-zip.org",
                isFeatured: true,
                publicId:
                    "70000000-0000-0000-0000-000000000001"),

            CreateApplication(
                id: 2,
                categoryId: 3,
                publisherId: 2,
                name: "VLC Media Player",
                slug: "vlc-media-player",
                description:
                    "A free and open-source multimedia player.",
                website: "https://www.videolan.org/vlc/",
                isFeatured: true,
                publicId:
                    "70000000-0000-0000-0000-000000000002"),

            CreateApplication(
                id: 3,
                categoryId: 1,
                publisherId: 3,
                name: "Mozilla Firefox",
                slug: "mozilla-firefox",
                description:
                    "A privacy-focused open-source web browser.",
                website: "https://www.mozilla.org/firefox/",
                isFeatured: true,
                publicId:
                    "70000000-0000-0000-0000-000000000003"),

            CreateApplication(
                id: 4,
                categoryId: 1,
                publisherId: 4,
                name: "Google Chrome",
                slug: "google-chrome",
                description:
                    "A fast web browser developed by Google.",
                website: "https://www.google.com/chrome/",
                isFeatured: true,
                publicId:
                    "70000000-0000-0000-0000-000000000004"),

            CreateApplication(
                id: 5,
                categoryId: 4,
                publisherId: 5,
                name: "Visual Studio Code",
                slug: "visual-studio-code",
                description:
                    "A lightweight source code editor developed by Microsoft.",
                website: "https://code.visualstudio.com",
                isFeatured: true,
                publicId:
                    "70000000-0000-0000-0000-000000000005"));
    }

    private static SoftwareApplication CreateApplication(
        int id,
        int categoryId,
        int publisherId,
        string name,
        string slug,
        string description,
        string website,
        bool isFeatured,
        string publicId)
    {
        return new SoftwareApplication
        {
            Id = id,
            PublicId = Guid.Parse(publicId),
            CategoryId = categoryId,
            PublisherId = publisherId,
            PlatformId = 1,
            Name = name,
            Slug = slug,
            Description = description,
            Website = website,
            IsFeatured = isFeatured,
            CreatedAt = SeedDate,
            IsActive = true
        };
    }

    private static void SeedApplicationTags(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationTag>().HasData(
            CreateApplicationTag(1, 5),
            CreateApplicationTag(1, 6),
            CreateApplicationTag(1, 7),

            CreateApplicationTag(2, 2),
            CreateApplicationTag(2, 5),

            CreateApplicationTag(3, 1),
            CreateApplicationTag(3, 5),

            CreateApplicationTag(4, 1),

            CreateApplicationTag(5, 3),
            CreateApplicationTag(5, 4));
    }

    private static ApplicationTag CreateApplicationTag(
        int softwareApplicationId,
        int tagId)
    {
        return new ApplicationTag
        {
            SoftwareApplicationId = softwareApplicationId,
            TagId = tagId
        };
    }
}