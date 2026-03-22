using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreOfTheMind.Migrations
{
    /// <inheritdoc />
    public partial class AddMercenariesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mercenaries",
                table: "ExpeditionState");

            migrationBuilder.CreateTable(
                name: "Mercenaries",
                columns: table => new
                {
                    MercenaryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Race = table.Column<string>(type: "text", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mercenaries", x => x.MercenaryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mercenaries_Type_Race",
                table: "Mercenaries",
                columns: new[] { "Type", "Race" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mercenaries");

            migrationBuilder.AddColumn<string>(
                name: "Mercenaries",
                table: "ExpeditionState",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
