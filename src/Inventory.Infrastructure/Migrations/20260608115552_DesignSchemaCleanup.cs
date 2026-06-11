using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DesignSchemaCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DesignMatchings_DesignPlateId",
                table: "DesignMatchings");

            migrationBuilder.Sql(@"ALTER TABLE ""DesignPlates"" ALTER COLUMN ""PlateNo"" TYPE integer USING ""PlateNo""::integer;");


            migrationBuilder.CreateIndex(
                name: "IX_DesignMatchings_DesignPlateId_MatchingNo",
                table: "DesignMatchings",
                columns: new[] { "DesignPlateId", "MatchingNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DesignMatchings_DesignPlateId_MatchingNo",
                table: "DesignMatchings");

            migrationBuilder.AlterColumn<string>(
                name: "PlateNo",
                table: "DesignPlates",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_DesignMatchings_DesignPlateId",
                table: "DesignMatchings",
                column: "DesignPlateId");
        }
    }
}
