using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TheatreOfTheMind.Migrations
{
    /// <inheritdoc />
    public partial class AddVttScenesBackup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VttSceneBackups",
                columns: table => new
                {
                    BackupId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BackupTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VttSceneBackups", x => x.BackupId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VttSceneBackups_BackupTimestamp",
                table: "VttSceneBackups",
                column: "BackupTimestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VttSceneBackups");
        }
    }
}
