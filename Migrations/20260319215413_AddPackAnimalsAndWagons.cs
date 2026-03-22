using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreOfTheMind.Migrations
{
    /// <inheritdoc />
    public partial class AddPackAnimalsAndWagons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wagons",
                columns: table => new
                {
                    WagonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wagons", x => x.WagonId);
                });

            migrationBuilder.CreateTable(
                name: "PackAnimals",
                columns: table => new
                {
                    PackAnimalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    AssignedWagonId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackAnimals", x => x.PackAnimalId);
                    table.ForeignKey(
                        name: "FK_PackAnimals_Wagons_AssignedWagonId",
                        column: x => x.AssignedWagonId,
                        principalTable: "Wagons",
                        principalColumn: "WagonId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackAnimals_AssignedWagonId",
                table: "PackAnimals",
                column: "AssignedWagonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackAnimals");

            migrationBuilder.DropTable(
                name: "Wagons");
        }
    }
}
