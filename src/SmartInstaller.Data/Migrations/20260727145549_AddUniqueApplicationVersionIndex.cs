using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInstaller.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueApplicationVersionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationVersions_SoftwareApplicationId",
                table: "ApplicationVersions");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVersions_SoftwareApplicationId_Version",
                table: "ApplicationVersions",
                columns: new[] { "SoftwareApplicationId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationVersions_SoftwareApplicationId_Version",
                table: "ApplicationVersions");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVersions_SoftwareApplicationId",
                table: "ApplicationVersions",
                column: "SoftwareApplicationId");
        }
    }
}
