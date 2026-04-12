using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreOfTheMind.Migrations
{
    /// <inheritdoc />
    public partial class AddFogOfWarColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FogEnabled",
                table: "VttScenes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FogReveals",
                table: "VttScenes",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<bool>(
                name: "FogEnabled",
                table: "VttSceneBackups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FogReveals",
                table: "VttSceneBackups",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FogEnabled",
                table: "VttScenes");

            migrationBuilder.DropColumn(
                name: "FogReveals",
                table: "VttScenes");

            migrationBuilder.DropColumn(
                name: "FogEnabled",
                table: "VttSceneBackups");

            migrationBuilder.DropColumn(
                name: "FogReveals",
                table: "VttSceneBackups");
        }
    }
}
