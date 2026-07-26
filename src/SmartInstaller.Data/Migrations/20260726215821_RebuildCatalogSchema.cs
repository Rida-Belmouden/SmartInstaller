using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartInstaller.Data.Migrations
{
    /// <inheritdoc />
    public partial class RebuildCatalogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationVersions_SoftwareApplicationId_Version",
                table: "ApplicationVersions");

            migrationBuilder.DeleteData(
                table: "Architectures",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Publishers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Publishers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Publishers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Publishers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "InstallerTypes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "InstallerTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "InstallerTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "InstallerProfiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "InstallerProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "InstallerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Categories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Categories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Architectures",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Architectures",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Architectures",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Architectures",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Architectures",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Architectures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ApplicationVersions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReleaseDate",
                table: "ApplicationVersions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsLatest",
                table: "ApplicationVersions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ApplicationVersions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ApplicationVersions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "ApplicationVersions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ApplicationVersions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Applications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PlatformId",
                table: "Applications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Applications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Applications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Platforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platforms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationTags",
                columns: table => new
                {
                    SoftwareApplicationId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationTags", x => new { x.SoftwareApplicationId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ApplicationTags_Applications_SoftwareApplicationId",
                        column: x => x.SoftwareApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Architectures",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "PublicId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("20000000-0000-0000-0000-000000000001"), null });

            migrationBuilder.UpdateData(
                table: "Architectures",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "PublicId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("20000000-0000-0000-0000-000000000002"), null });

            migrationBuilder.UpdateData(
                table: "Architectures",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "PublicId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("20000000-0000-0000-0000-000000000003"), null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "PublicId", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Browsers", new Guid("40000000-0000-0000-0000-000000000001"), "browsers", null },
                    { 2, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Messaging", new Guid("40000000-0000-0000-0000-000000000002"), "messaging", null },
                    { 3, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Media", new Guid("40000000-0000-0000-0000-000000000003"), "media", null },
                    { 4, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Development", new Guid("40000000-0000-0000-0000-000000000004"), "development", null },
                    { 5, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Utilities", new Guid("40000000-0000-0000-0000-000000000005"), "utilities", null },
                    { 6, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Security", new Guid("40000000-0000-0000-0000-000000000006"), "security", null },
                    { 7, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Compression", new Guid("40000000-0000-0000-0000-000000000007"), "compression", null },
                    { 8, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Cloud Storage", new Guid("40000000-0000-0000-0000-000000000008"), "cloud-storage", null }
                });

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "PublicId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("30000000-0000-0000-0000-000000000001"), null });

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "PublicId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("30000000-0000-0000-0000-000000000002"), null });

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "PublicId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("30000000-0000-0000-0000-000000000003"), null });

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "Description", "Name", "PublicId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), null, "ZIP", new Guid("30000000-0000-0000-0000-000000000004"), null });

            migrationBuilder.InsertData(
                table: "Platforms",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "PublicId", "Slug", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Microsoft Windows operating system", true, "Windows", new Guid("10000000-0000-0000-0000-000000000001"), "windows", null });

            migrationBuilder.CreateIndex(
                name: "IX_Architectures_PublicId",
                table: "Architectures",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVersions_PublicId",
                table: "ApplicationVersions",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_PlatformId",
                table: "Applications",
                column: "PlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationTags_TagId",
                table: "ApplicationTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_Name",
                table: "Platforms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_PublicId",
                table: "Platforms",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_Slug",
                table: "Platforms",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_PublicId",
                table: "Tags",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Slug",
                table: "Tags",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Platforms_PlatformId",
                table: "Applications",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Platforms_PlatformId",
                table: "Applications");

            migrationBuilder.DropTable(
                name: "ApplicationTags");

            migrationBuilder.DropTable(
                name: "Platforms");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Architectures_PublicId",
                table: "Architectures");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationVersions_PublicId",
                table: "ApplicationVersions");

            migrationBuilder.DropIndex(
                name: "IX_Applications_PlatformId",
                table: "Applications");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Publishers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Publishers");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Publishers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Publishers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "InstallerTypes");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "InstallerTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "InstallerTypes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "InstallerProfiles");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "InstallerProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "InstallerProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Architectures");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Architectures");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Architectures");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ApplicationVersions");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "ApplicationVersions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ApplicationVersions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Applications");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Architectures",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Architectures",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Architectures",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ApplicationVersions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReleaseDate",
                table: "ApplicationVersions",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsLatest",
                table: "ApplicationVersions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ApplicationVersions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.UpdateData(
                table: "Architectures",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "32-bit Windows architecture");

            migrationBuilder.UpdateData(
                table: "Architectures",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "64-bit Windows architecture");

            migrationBuilder.UpdateData(
                table: "Architectures",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "64-bit ARM architecture");

            migrationBuilder.InsertData(
                table: "Architectures",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[] { 4, "Architecture-independent installer", true, "Any" });

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Windows executable installer");

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Microsoft Windows Installer package");

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Modern Microsoft application package");

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Windows application package", "APPX" });

            migrationBuilder.InsertData(
                table: "InstallerTypes",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[] { 5, "Portable compressed application", true, "ZIP" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVersions_SoftwareApplicationId_Version",
                table: "ApplicationVersions",
                columns: new[] { "SoftwareApplicationId", "Version" },
                unique: true);
        }
    }
}
