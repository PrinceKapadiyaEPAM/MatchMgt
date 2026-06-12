using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddB2BUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Party",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RestockDate",
                table: "Catalogues",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "B2BUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    PartyId = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2BUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_B2BUsers_Party_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Party",
                        principalColumn: "PartyId");
                });

            migrationBuilder.CreateTable(
                name: "CataloguePartyPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CatalogueId = table.Column<int>(type: "integer", nullable: false),
                    PartyId = table.Column<int>(type: "integer", nullable: false),
                    OverridePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CataloguePartyPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CataloguePartyPrices_Catalogues_CatalogueId",
                        column: x => x.CatalogueId,
                        principalTable: "Catalogues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CataloguePartyPrices_Party_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Party",
                        principalColumn: "PartyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_B2BUsers_Email",
                table: "B2BUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_B2BUsers_PartyId",
                table: "B2BUsers",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_CataloguePartyPrices_CatalogueId_PartyId",
                table: "CataloguePartyPrices",
                columns: new[] { "CatalogueId", "PartyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CataloguePartyPrices_PartyId",
                table: "CataloguePartyPrices",
                column: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "B2BUsers");

            migrationBuilder.DropTable(
                name: "CataloguePartyPrices");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Party");

            migrationBuilder.DropColumn(
                name: "RestockDate",
                table: "Catalogues");
        }
    }
}
