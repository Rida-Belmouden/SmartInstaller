using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInstaller.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallerProfileFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InstallerProfiles_ApplicationVersionId",
                table: "InstallerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Architectures_PublicId",
                table: "Architectures");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "InstallerTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "InstallerTypes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "InstallerTypes",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Sha256",
                table: "InstallerProfiles",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nchar(64)",
                oldFixedLength: true,
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "InstallerProfiles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "DownloadUrl",
                table: "InstallerProfiles",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "InstallerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPortable",
                table: "InstallerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Architectures",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "PublicId", "UpdatedAt" },
                values: new object[] { 4, new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Architecture-independent installer", true, "Any", new Guid("20000000-0000-0000-0000-000000000004"), null });

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
                value: "Windows Installer package");

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Microsoft application package");

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Portable compressed archive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Architectures",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "InstallerProfiles");

            migrationBuilder.DropColumn(
                name: "IsPortable",
                table: "InstallerProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "InstallerTypes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "InstallerTypes",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "InstallerTypes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Sha256",
                table: "InstallerProfiles",
                type: "nchar(64)",
                fixedLength: true,
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "InstallerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "DownloadUrl",
                table: "InstallerProfiles",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Architectures",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Architectures",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Architectures",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "Architectures",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: null);

            migrationBuilder.UpdateData(
                table: "InstallerTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_InstallerProfiles_ApplicationVersionId",
                table: "InstallerProfiles",
                column: "ApplicationVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Architectures_PublicId",
                table: "Architectures",
                column: "PublicId",
                unique: true);
        }
    }
}
