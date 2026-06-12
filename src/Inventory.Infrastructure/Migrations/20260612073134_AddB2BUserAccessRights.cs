using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddB2BUserAccessRights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowCatalogue",
                table: "B2BUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPrices",
                table: "B2BUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowStock",
                table: "B2BUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // Backfill existing rows to full access
            migrationBuilder.Sql(@"UPDATE ""B2BUsers"" SET ""ShowCatalogue"" = TRUE, ""ShowPrices"" = TRUE, ""ShowStock"" = TRUE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowCatalogue",
                table: "B2BUsers");

            migrationBuilder.DropColumn(
                name: "ShowPrices",
                table: "B2BUsers");

            migrationBuilder.DropColumn(
                name: "ShowStock",
                table: "B2BUsers");
        }
    }
}
