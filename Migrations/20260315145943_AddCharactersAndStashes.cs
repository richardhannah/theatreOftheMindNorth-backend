using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreOfTheMind.Migrations
{
    /// <inheritdoc />
    public partial class AddCharactersAndStashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PlayerName = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false),
                    Alignment = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Str = table.Column<int>(type: "integer", nullable: false),
                    Int = table.Column<int>(type: "integer", nullable: false),
                    Wis = table.Column<int>(type: "integer", nullable: false),
                    Dex = table.Column<int>(type: "integer", nullable: false),
                    Con = table.Column<int>(type: "integer", nullable: false),
                    Cha = table.Column<int>(type: "integer", nullable: false),
                    Hp = table.Column<int>(type: "integer", nullable: false),
                    MaxHp = table.Column<int>(type: "integer", nullable: false),
                    Ac = table.Column<int>(type: "integer", nullable: false),
                    Thac0 = table.Column<int>(type: "integer", nullable: false),
                    Movement = table.Column<string>(type: "text", nullable: false),
                    Initiative = table.Column<string>(type: "text", nullable: false),
                    SavDeathPoison = table.Column<int>(type: "integer", nullable: false),
                    SavWands = table.Column<int>(type: "integer", nullable: false),
                    SavParalysisStone = table.Column<int>(type: "integer", nullable: false),
                    SavBreathAttack = table.Column<int>(type: "integer", nullable: false),
                    SavSpellsStaffRod = table.Column<int>(type: "integer", nullable: false),
                    ClassAbilities = table.Column<string>(type: "jsonb", nullable: false),
                    Skills = table.Column<string>(type: "jsonb", nullable: false),
                    WeaponMasteries = table.Column<string>(type: "jsonb", nullable: false),
                    PreparedSpells = table.Column<string>(type: "jsonb", nullable: false),
                    Spellbook = table.Column<string>(type: "jsonb", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.CharacterId);
                    table.ForeignKey(
                        name: "FK_Characters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stashes",
                columns: table => new
                {
                    StashId = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Removable = table.Column<bool>(type: "boolean", nullable: false),
                    Shared = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Platinum = table.Column<int>(type: "integer", nullable: false),
                    Gold = table.Column<int>(type: "integer", nullable: false),
                    Electrum = table.Column<int>(type: "integer", nullable: false),
                    Silver = table.Column<int>(type: "integer", nullable: false),
                    Copper = table.Column<int>(type: "integer", nullable: false),
                    Equipment = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stashes", x => x.StashId);
                    table.ForeignKey(
                        name: "FK_Stashes_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "CharacterId");
                });

            migrationBuilder.InsertData(
                table: "Stashes",
                columns: new[] { "StashId", "CharacterId", "Copper", "Electrum", "Equipment", "Gold", "Name", "Platinum", "Removable", "Shared", "Silver", "SortOrder" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), null, 0, 0, "[]", 0, "Expedition Caravan", 0, false, true, 0, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_UserId",
                table: "Characters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Stashes_CharacterId",
                table: "Stashes",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Stashes_Shared",
                table: "Stashes",
                column: "Shared");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stashes");

            migrationBuilder.DropTable(
                name: "Characters");
        }
    }
}
