using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogueIdToDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Catalogues_Code",
                table: "Catalogues");

            migrationBuilder.DropColumn(
                name: "Quality",
                table: "DispatchEntries");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Catalogues");

            migrationBuilder.DropColumn(
                name: "Packing",
                table: "Catalogues");

            migrationBuilder.AddColumn<int>(
                name: "CatalogueId",
                table: "DispatchEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchEntries_CatalogueId",
                table: "DispatchEntries",
                column: "CatalogueId");

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchEntries_Catalogues_CatalogueId",
                table: "DispatchEntries",
                column: "CatalogueId",
                principalTable: "Catalogues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DispatchEntries_Catalogues_CatalogueId",
                table: "DispatchEntries");

            migrationBuilder.DropIndex(
                name: "IX_DispatchEntries_CatalogueId",
                table: "DispatchEntries");

            migrationBuilder.DropColumn(
                name: "CatalogueId",
                table: "DispatchEntries");

            migrationBuilder.AddColumn<string>(
                name: "Quality",
                table: "DispatchEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Catalogues",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Packing",
                table: "Catalogues",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Catalogues_Code",
                table: "Catalogues",
                column: "Code",
                unique: true);
        }
    }
}
