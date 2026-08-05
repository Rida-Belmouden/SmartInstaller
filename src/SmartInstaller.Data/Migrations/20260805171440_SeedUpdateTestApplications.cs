using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartInstaller.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedUpdateTestApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ApplicationVersions",
                columns: new[] { "Id", "CreatedAt", "IsActive", "IsLatest", "PublicId", "ReleaseDate", "SoftwareApplicationId", "UpdatedAt", "Version" },
                values: new object[] { 1001, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, true, new Guid("80000000-0000-0000-0000-000000000001"), null, 2, null, "3.0.23" });

            migrationBuilder.InsertData(
                table: "Publishers",
                columns: new[] { "Id", "CreatedAt", "IsActive", "IsVerified", "LogoUrl", "Name", "PublicId", "UpdatedAt", "Website" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, true, null, "Notepad++ Team", new Guid("50000000-0000-0000-0000-000000000006"), null, "https://notepad-plus-plus.org" },
                    { 7, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, true, null, "Muse Group", new Guid("50000000-0000-0000-0000-000000000007"), null, "https://www.audacityteam.org" }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "PublicId", "Slug", "UpdatedAt" },
                values: new object[] { 8, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Audio recording and editing software", true, "Audio Editor", new Guid("60000000-0000-0000-0000-000000000008"), "audio-editor", null });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IconUrl", "IsActive", "IsFeatured", "Name", "PlatformId", "PublicId", "PublisherId", "Slug", "UpdatedAt", "Website" },
                values: new object[,]
                {
                    { 6, 4, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "A free source code editor and Notepad replacement.", null, true, true, "Notepad++", 1, new Guid("70000000-0000-0000-0000-000000000006"), 6, "notepad-plus-plus", null, "https://notepad-plus-plus.org" },
                    { 7, 3, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "A free, open-source multi-track audio editor and recorder.", null, true, true, "Audacity", 1, new Guid("70000000-0000-0000-0000-000000000007"), 7, "audacity", null, "https://www.audacityteam.org" }
                });

            migrationBuilder.InsertData(
                table: "InstallerProfiles",
                columns: new[] { "Id", "ApplicationVersionId", "ArchitectureId", "CreatedAt", "DownloadUrl", "FileSizeBytes", "InstallerTypeId", "IsActive", "IsEnabled", "PublicId", "RequiresAdministrator", "Sha256", "SilentInstallArguments", "SilentUninstallArguments", "UpdatedAt" },
                values: new object[,]
                {
                    { 1001, 1001, 2, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "https://get.videolan.org/vlc/3.0.23/win64/vlc-3.0.23-win64.exe", 45948080L, 1, true, true, new Guid("90000000-0000-0000-0000-000000000001"), true, "3b52a763a789562399f2e31063c9e545ca0ff28fa464e00b4af661fd62a6260d", "/S", null, null },
                    { 1002, 1001, 1, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "https://get.videolan.org/vlc/3.0.23/win32/vlc-3.0.23-win32.exe", 44568024L, 1, true, true, new Guid("90000000-0000-0000-0000-000000000002"), true, "0eef8f155f5fe6f70e9e1ad6c399cbeaaa1b3390b2281091ed1fb75f39fedae3", "/S", null, null }
                });

            migrationBuilder.InsertData(
                table: "ApplicationTags",
                columns: new[] { "SoftwareApplicationId", "TagId" },
                values: new object[,]
                {
                    { 6, 3 },
                    { 6, 4 },
                    { 6, 5 },
                    { 7, 5 },
                    { 7, 8 }
                });

            migrationBuilder.InsertData(
                table: "ApplicationVersions",
                columns: new[] { "Id", "CreatedAt", "IsActive", "IsLatest", "PublicId", "ReleaseDate", "SoftwareApplicationId", "UpdatedAt", "Version" },
                values: new object[,]
                {
                    { 1002, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, true, new Guid("80000000-0000-0000-0000-000000000002"), null, 6, null, "8.9.7" },
                    { 1003, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, true, new Guid("80000000-0000-0000-0000-000000000003"), null, 7, null, "3.7.8" }
                });

            migrationBuilder.InsertData(
                table: "InstallerProfiles",
                columns: new[] { "Id", "ApplicationVersionId", "ArchitectureId", "CreatedAt", "DownloadUrl", "FileSizeBytes", "InstallerTypeId", "IsActive", "IsEnabled", "PublicId", "RequiresAdministrator", "Sha256", "SilentInstallArguments", "SilentUninstallArguments", "UpdatedAt" },
                values: new object[,]
                {
                    { 1003, 1002, 2, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.9.7/npp.8.9.7.Installer.x64.exe", 6933184L, 1, true, true, new Guid("90000000-0000-0000-0000-000000000003"), true, "1884e093bae261c4942210334e1f2eae71354913e4ded3cc1a4a18c5320741ec", "/S", null, null },
                    { 1004, 1002, 1, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.9.7/npp.8.9.7.Installer.exe", 6788296L, 1, true, true, new Guid("90000000-0000-0000-0000-000000000004"), true, "9b89aa3221fdbce6ec2ee6f7d07de9fd7df2fbb7e7ce6d2f24e34ca347157dcf", "/S", null, null },
                    { 1005, 1003, 2, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "https://github.com/audacity/audacity/releases/download/Audacity-3.7.8/audacity-win-3.7.8-64bit.exe", 20400664L, 1, true, true, new Guid("90000000-0000-0000-0000-000000000005"), true, "4a6d77f023c0209a396fcf2f3c7c04e240b6ba9897b3231a6ed18e14c10caa16", "/VERYSILENT /NORESTART", null, null },
                    { 1006, 1003, 1, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "https://github.com/audacity/audacity/releases/download/Audacity-3.7.8/audacity-win-3.7.8-32bit.exe", 18940432L, 1, true, true, new Guid("90000000-0000-0000-0000-000000000006"), true, "c0482d84a05ddd26905d010daff32b79eba18673489d2cb90ee87a386d24c74f", "/VERYSILENT /NORESTART", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 6, 3 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 6, 4 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 6, 5 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 7, 5 });

            migrationBuilder.DeleteData(
                table: "ApplicationTags",
                keyColumns: new[] { "SoftwareApplicationId", "TagId" },
                keyValues: new object[] { 7, 8 });

            migrationBuilder.DeleteData(
                table: "InstallerProfiles",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "InstallerProfiles",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.DeleteData(
                table: "InstallerProfiles",
                keyColumn: "Id",
                keyValue: 1003);

            migrationBuilder.DeleteData(
                table: "InstallerProfiles",
                keyColumn: "Id",
                keyValue: 1004);

            migrationBuilder.DeleteData(
                table: "InstallerProfiles",
                keyColumn: "Id",
                keyValue: 1005);

            migrationBuilder.DeleteData(
                table: "InstallerProfiles",
                keyColumn: "Id",
                keyValue: 1006);

            migrationBuilder.DeleteData(
                table: "ApplicationVersions",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "ApplicationVersions",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.DeleteData(
                table: "ApplicationVersions",
                keyColumn: "Id",
                keyValue: 1003);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
