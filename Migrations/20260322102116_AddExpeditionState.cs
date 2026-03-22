using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreOfTheMind.Migrations
{
    /// <inheritdoc />
    public partial class AddExpeditionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpeditionState",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ForemanName = table.Column<string>(type: "text", nullable: false),
                    ForemanTokenId = table.Column<string>(type: "text", nullable: false),
                    Mercenaries = table.Column<string>(type: "jsonb", nullable: false),
                    DisabledRaces = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpeditionState", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpeditionState");
        }
    }
}
