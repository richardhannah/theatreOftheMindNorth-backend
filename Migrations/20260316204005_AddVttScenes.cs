using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreOfTheMind.Migrations
{
    /// <inheritdoc />
    public partial class AddVttScenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VttScenes",
                columns: table => new
                {
                    SceneId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MapId = table.Column<string>(type: "text", nullable: false),
                    GridW = table.Column<int>(type: "integer", nullable: false),
                    GridH = table.Column<int>(type: "integer", nullable: false),
                    GridOffsetX = table.Column<double>(type: "double precision", nullable: false),
                    GridOffsetY = table.Column<double>(type: "double precision", nullable: false),
                    GridColor = table.Column<string>(type: "text", nullable: false),
                    GridOpacity = table.Column<double>(type: "double precision", nullable: false),
                    GridThickness = table.Column<int>(type: "integer", nullable: false),
                    Counters = table.Column<string>(type: "jsonb", nullable: false),
                    NextCounterId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VttScenes", x => x.SceneId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VttScenes");
        }
    }
}
