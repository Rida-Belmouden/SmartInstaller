using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartInstaller.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedApplicationCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Publishers",
                columns: new[] { "Id", "CreatedAt", "IsActive", "IsVerified", "LogoUrl", "Name", "PublicId", "UpdatedAt", "Website" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, true, null, "Igor Pavlov", new Guid("50000000-0000-0000-0000-000000000001"), null, "https://www.7-zip.org" },
                    { 2, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, true, null, "VideoLAN", new Guid("50000000-0000-0000-0000-000000000002"), null, "https://www.videolan.org" },
                    { 3, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, true, null, "Mozilla", new Guid("50000000-0000-0000-0000-000000000003"), null, "https://www.mozilla.org" },
                    { 4, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, true, null, "Google", new Guid("50000000-0000-0000-0000-000000000004"), null, "https://www.google.com" },
                    { 5, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, true, null, "Microsoft", new Guid("50000000-0000-0000-0000-000000000005"), null, "https://www.microsoft.com" }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "PublicId", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Web browsing software", true, "Browser", new Guid("60000000-0000-0000-0000-000000000001"), "browser", null },
                    { 2, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Audio and video playback software", true, "Media Player", new Guid("60000000-0000-0000-0000-000000000002"), "media-player", null },
                    { 3, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Software development tools", true, "Developer Tool", new Guid("60000000-0000-0000-0000-000000000003"), "developer-tool", null },
                    { 4, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Source code editing software", true, "Code Editor", new Guid("60000000-0000-0000-0000-000000000004"), "code-editor", null },
                    { 5, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Open-source software", true, "Open Source", new Guid("60000000-0000-0000-0000-000000000005"), "open-source", null },
                    { 6, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "File compression and extraction software", true, "Compression", new Guid("60000000-0000-0000-0000-000000000006"), "compression", null },
                    { 7, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "General system utility", true, "Utility", new Guid("60000000-0000-0000-0000-000000000007"), "utility", null }
                });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IconUrl", "IsActive", "IsFeatured", "Name", "PlatformId", "PublicId", "PublisherId", "Slug", "UpdatedAt", "Website" },
                values: new object[,]
                {
                    { 1, 7, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "A file archiver with a high compression ratio.", null, true, true, "7-Zip", 1, new Guid("70000000-0000-0000-0000-000000000001"), 1, "7-zip", null, "https://www.7-zip.org" },
                    { 2, 3, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "A free and open-source multimedia player.", null, true, true, "VLC Media Player", 1, new Guid("70000000-0000-0000-0000-000000000002"), 2, "vlc-media-player", null, "https://www.videolan.org/vlc/" },
                    { 3, 1, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "A privacy-focused open-source web browser.", null, true, true, "Mozilla Firefox", 1, new Guid("70000000-0000-0000-0000-000000000003"), 3, "mozilla-firefox", null, "https://www.mozilla.org/firefox/" },
                    { 4, 1, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "A fast web browser developed by Google.", null, true, true, "Google Chrome", 1, new Guid("70000000-0000-0000-0000-000000000004"), 4, "google-chrome", null, "https://www.google.com/chrome/" },
                    { 5, 4, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "A lightweight source code editor developed by Microsoft.", null, true, true, "Visual Studio Code", 1, new Guid("70000000-0000-0000-0000-000000000005"), 5, "visual-studio-code", null, "https://code.visualstudio.com" }
                });

            migrationBuilder.InsertData(
                table: "ApplicationTags",
                columns: new[] { "SoftwareApplicationId", "TagId" },
                values: new object[,]
                {
                    { 1, 5 },
                    { 1, 6 },
                    { 1, 7 },
                    { 2, 2 },
                    { 2, 5 },
                    { 3, 1 },
                    { 3, 5 },
                    { 4, 1 },
                    { 5, 3 },
                    { 5, 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 1, 5 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 1, 6 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 1, 7 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 2, 5 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 3, 5 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 4, 1 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 5, 3 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 5, 4 });

            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
