using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartInstaller.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandApplicationCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Categories_CategoryId",
                table: "Applications");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "Applications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Applications",
                type: "nvarchar(170)",
                maxLength: 170,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Applications",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Applications",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "IconUrl",
                table: "Applications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Applications",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Applications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PublisherId",
                table: "Applications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ApplicationVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoftwareApplicationId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationVersions_Applications_SoftwareApplicationId",
                        column: x => x.SoftwareApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Architectures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Architectures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstallerTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstallerTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstallerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationVersionId = table.Column<int>(type: "int", nullable: false),
                    InstallerTypeId = table.Column<int>(type: "int", nullable: false),
                    ArchitectureId = table.Column<int>(type: "int", nullable: false),
                    DownloadUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Sha256 = table.Column<string>(type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    SilentInstallArguments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SilentUninstallArguments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequiresAdministrator = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstallerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstallerProfiles_ApplicationVersions_ApplicationVersionId",
                        column: x => x.ApplicationVersionId,
                        principalTable: "ApplicationVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstallerProfiles_Architectures_ArchitectureId",
                        column: x => x.ArchitectureId,
                        principalTable: "Architectures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstallerProfiles_InstallerTypes_InstallerTypeId",
                        column: x => x.InstallerTypeId,
                        principalTable: "InstallerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Architectures",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "32-bit Windows architecture", true, "x86" },
                    { 2, "64-bit Windows architecture", true, "x64" },
                    { 3, "64-bit ARM architecture", true, "ARM64" },
                    { 4, "Architecture-independent installer", true, "Any" }
                });

            migrationBuilder.InsertData(
                table: "InstallerTypes",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Windows executable installer", true, "EXE" },
                    { 2, "Microsoft Windows Installer package", true, "MSI" },
                    { 3, "Modern Microsoft application package", true, "MSIX" },
                    { 4, "Windows application package", true, "APPX" },
                    { 5, "Portable compressed application", true, "ZIP" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Name",
                table: "Applications",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_PublisherId",
                table: "Applications",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Slug",
                table: "Applications",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVersions_SoftwareApplicationId",
                table: "ApplicationVersions",
                column: "SoftwareApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVersions_SoftwareApplicationId_Version",
                table: "ApplicationVersions",
                columns: new[] { "SoftwareApplicationId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Architectures_Name",
                table: "Architectures",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstallerProfiles_ApplicationVersionId",
                table: "InstallerProfiles",
                column: "ApplicationVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallerProfiles_ApplicationVersionId_InstallerTypeId_ArchitectureId",
                table: "InstallerProfiles",
                columns: new[] { "ApplicationVersionId", "InstallerTypeId", "ArchitectureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstallerProfiles_ArchitectureId",
                table: "InstallerProfiles",
                column: "ArchitectureId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallerProfiles_InstallerTypeId",
                table: "InstallerProfiles",
                column: "InstallerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallerTypes_Name",
                table: "InstallerTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Publishers_Name",
                table: "Publishers",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Categories_CategoryId",
                table: "Applications",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Publishers_PublisherId",
                table: "Applications",
                column: "PublisherId",
                principalTable: "Publishers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Categories_CategoryId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Publishers_PublisherId",
                table: "Applications");

            migrationBuilder.DropTable(
                name: "InstallerProfiles");

            migrationBuilder.DropTable(
                name: "Publishers");

            migrationBuilder.DropTable(
                name: "ApplicationVersions");

            migrationBuilder.DropTable(
                name: "Architectures");

            migrationBuilder.DropTable(
                name: "InstallerTypes");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Applications_Name",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_PublisherId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_Slug",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "PublisherId",
                table: "Applications");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Categories",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(170)",
                oldMaxLength: 170);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Applications",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "IconUrl",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Categories_CategoryId",
                table: "Applications",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
