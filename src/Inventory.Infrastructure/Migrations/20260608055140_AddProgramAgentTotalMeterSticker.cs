using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramAgentTotalMeterSticker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Agent",
                table: "Program",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sticker",
                table: "Program",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMeter",
                table: "Program",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Agent",
                table: "Program");

            migrationBuilder.DropColumn(
                name: "Sticker",
                table: "Program");

            migrationBuilder.DropColumn(
                name: "TotalMeter",
                table: "Program");
        }
    }
}
