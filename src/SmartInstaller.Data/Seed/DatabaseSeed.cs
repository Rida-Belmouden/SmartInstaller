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
                PublicId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "Windows",
                Slug = "windows",
                Description = "Microsoft Windows operating system",
                CreatedAt = SeedDate,
                UpdatedAt = null,
                IsActive = true
            });
    }

    private static void SeedArchitectures(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Architecture>().HasData(
            CreateArchitecture(1, "x86", "32-bit Windows architecture", "20000000-0000-0000-0000-000000000001"),
            CreateArchitecture(2, "x64", "64-bit Windows architecture", "20000000-0000-0000-0000-000000000002"),
            CreateArchitecture(3, "ARM64", "64-bit ARM architecture", "20000000-0000-0000-0000-000000000003"),
            CreateArchitecture(4, "Any", "Architecture-independent installer", "20000000-0000-0000-0000-000000000004"));
    }

    private static Architecture CreateArchitecture(
        int id,
        string name,
        string description,
        string publicId)
    {
        return new Architecture
        {
            Id = id,
            PublicId = Guid.Parse(publicId),
            Name = name,
            Description = description,
            CreatedAt = SeedDate,
            UpdatedAt = null,
            IsActive = true
        };
    }

    private static void SeedInstallerTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InstallerType>().HasData(
            CreateInstallerType(1, "EXE", "Windows executable installer", "30000000-0000-0000-0000-000000000001"),
            CreateInstallerType(2, "MSI", "Windows Installer package", "30000000-0000-0000-0000-000000000002"),
            CreateInstallerType(3, "MSIX", "Microsoft application package", "30000000-0000-0000-0000-000000000003"),
            CreateInstallerType(4, "ZIP", "Portable compressed archive", "30000000-0000-0000-0000-000000000004"));
    }

    private static InstallerType CreateInstallerType(
        int id,
        string name,
        string description,
        string publicId)
    {
        return new InstallerType
        {
            Id = id,
            PublicId = Guid.Parse(publicId),
            Name = name,
            Description = description,
            CreatedAt = SeedDate,
            UpdatedAt = null,
            IsActive = true
        };
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            CreateCategory(1, "Browsers", "browsers", "40000000-0000-0000-0000-000000000001"),
            CreateCategory(2, "Messaging", "messaging", "40000000-0000-0000-0000-000000000002"),
            CreateCategory(3, "Media", "media", "40000000-0000-0000-0000-000000000003"),
            CreateCategory(4, "Development", "development", "40000000-0000-0000-0000-000000000004"),
            CreateCategory(5, "Utilities", "utilities", "40000000-0000-0000-0000-000000000005"),
            CreateCategory(6, "Security", "security", "40000000-0000-0000-0000-000000000006"),
            CreateCategory(7, "Compression", "compression", "40000000-0000-0000-0000-000000000007"),
            CreateCategory(8, "Cloud Storage", "cloud-storage", "40000000-0000-0000-0000-000000000008"));
    }

    private static Category CreateCategory(int id, string name, string slug, string publicId)
    {
        return new Category
        {
            Id = id,
            PublicId = Guid.Parse(publicId),
            Name = name,
            Slug = slug,
            CreatedAt = SeedDate,
            UpdatedAt = null,
            IsActive = true
        };
    }

    private static void SeedPublishers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Publisher>().HasData(
            CreatePublisher(1, "Igor Pavlov", "https://www.7-zip.org", "50000000-0000-0000-0000-000000000001"),
            CreatePublisher(2, "VideoLAN", "https://www.videolan.org", "50000000-0000-0000-0000-000000000002"),
            CreatePublisher(3, "Mozilla", "https://www.mozilla.org", "50000000-0000-0000-0000-000000000003"),
            CreatePublisher(4, "Google", "https://www.google.com", "50000000-0000-0000-0000-000000000004"),
            CreatePublisher(5, "Microsoft", "https://www.microsoft.com", "50000000-0000-0000-0000-000000000005"));
    }

    private static Publisher CreatePublisher(int id, string name, string website, string publicId)
    {
        return new Publisher
        {
            Id = id,
            PublicId = Guid.Parse(publicId),
            Name = name,
            Website = website,
            IsVerified = true,
            CreatedAt = SeedDate,
            UpdatedAt = null,
            IsActive = true
        };
    }

    private static void SeedTags(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>().HasData(
            CreateTag(1, "Browser", "browser", "Web browsing software", "60000000-0000-0000-0000-000000000001"),
            CreateTag(2, "Media Player", "media-player", "Audio and video playback software", "60000000-0000-0000-0000-000000000002"),
            CreateTag(3, "Developer Tool", "developer-tool", "Software development tools", "60000000-0000-0000-0000-000000000003"),
            CreateTag(4, "Code Editor", "code-editor", "Source code editing software", "60000000-0000-0000-0000-000000000004"),
            CreateTag(5, "Open Source", "open-source", "Open-source software", "60000000-0000-0000-0000-000000000005"),
            CreateTag(6, "Compression", "compression", "File compression and extraction software", "60000000-0000-0000-0000-000000000006"),
            CreateTag(7, "Utility", "utility", "General system utility", "60000000-0000-0000-0000-000000000007"));
    }

    private static Tag CreateTag(int id, string name, string slug, string description, string publicId)
    {
        return new Tag
        {
            Id = id,
            PublicId = Guid.Parse(publicId),
            Name = name,
            Slug = slug,
            Description = description,
            CreatedAt = SeedDate,
            UpdatedAt = null,
            IsActive = true
        };
    }

    private static void SeedApplications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SoftwareApplication>().HasData(
            CreateApplication(1, 7, 1, "7-Zip", "7-zip", "A file archiver with a high compression ratio.", "https://www.7-zip.org", true, "70000000-0000-0000-0000-000000000001"),
            CreateApplication(2, 3, 2, "VLC Media Player", "vlc-media-player", "A free and open-source multimedia player.", "https://www.videolan.org/vlc/", true, "70000000-0000-0000-0000-000000000002"),
            CreateApplication(3, 1, 3, "Mozilla Firefox", "mozilla-firefox", "A privacy-focused open-source web browser.", "https://www.mozilla.org/firefox/", true, "70000000-0000-0000-0000-000000000003"),
            CreateApplication(4, 1, 4, "Google Chrome", "google-chrome", "A fast web browser developed by Google.", "https://www.google.com/chrome/", true, "70000000-0000-0000-0000-000000000004"),
            CreateApplication(5, 4, 5, "Visual Studio Code", "visual-studio-code", "A lightweight source code editor developed by Microsoft.", "https://code.visualstudio.com", true, "70000000-0000-0000-0000-000000000005"));
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
            UpdatedAt = null,
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

    private static ApplicationTag CreateApplicationTag(int softwareApplicationId, int tagId)
    {
        return new ApplicationTag
        {
            SoftwareApplicationId = softwareApplicationId,
            TagId = tagId
        };
    }
}