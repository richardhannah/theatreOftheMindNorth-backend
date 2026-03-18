using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreOfTheMind.Migrations
{
    /// <inheritdoc />
    public partial class CounterIdsToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextCounterId",
                table: "VttScenes");

            // Migrate counter IDs in JSONB from integers to GUIDs
            migrationBuilder.Sql(@"
                UPDATE ""VttScenes""
                SET ""Counters"" = (
                    SELECT COALESCE(jsonb_agg(
                        jsonb_set(elem, '{id}', to_jsonb(gen_random_uuid()::text))
                    ), '[]'::jsonb)
                    FROM jsonb_array_elements(""Counters"") AS elem
                )
                WHERE ""Counters"" != '[]'::jsonb;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "NextCounterId",
                table: "VttScenes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
