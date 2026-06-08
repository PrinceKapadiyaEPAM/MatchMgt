using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddToPartyIdToDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToParty",
                table: "DispatchEntries");

            migrationBuilder.AddColumn<int>(
                name: "ToPartyId",
                table: "DispatchEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchEntries_ToPartyId",
                table: "DispatchEntries",
                column: "ToPartyId");

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchEntries_Party_ToPartyId",
                table: "DispatchEntries",
                column: "ToPartyId",
                principalTable: "Party",
                principalColumn: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DispatchEntries_Party_ToPartyId",
                table: "DispatchEntries");

            migrationBuilder.DropIndex(
                name: "IX_DispatchEntries_ToPartyId",
                table: "DispatchEntries");

            migrationBuilder.DropColumn(
                name: "ToPartyId",
                table: "DispatchEntries");

            migrationBuilder.AddColumn<string>(
                name: "ToParty",
                table: "DispatchEntries",
                type: "text",
                nullable: true);
        }
    }
}
