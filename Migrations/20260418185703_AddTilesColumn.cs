using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreOfTheMind.Migrations
{
    /// <inheritdoc />
    public partial class AddTilesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tiles",
                table: "VttScenes",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Tiles",
                table: "VttSceneBackups",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tiles",
                table: "VttScenes");

            migrationBuilder.DropColumn(
                name: "Tiles",
                table: "VttSceneBackups");
        }
    }
}
