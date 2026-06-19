using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Catalogue_descriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LongDescription",
                table: "Catalogues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "Catalogues",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LongDescription",
                table: "Catalogues");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "Catalogues");
        }
    }
}
