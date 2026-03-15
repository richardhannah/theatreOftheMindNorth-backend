using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreOfTheMind.Migrations
{
    /// <inheritdoc />
    public partial class PromoteSiteAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var siteAdmin = Environment.GetEnvironmentVariable("SITE_ADMIN");
            if (!string.IsNullOrWhiteSpace(siteAdmin))
            {
                migrationBuilder.Sql(
                    $"""
                    UPDATE "Users" SET "Role" = 'Admin'
                    WHERE "UserId" = (
                        SELECT "UserId" FROM "Logins" WHERE "Username" = '{siteAdmin.Replace("'", "''")}'
                    );
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var siteAdmin = Environment.GetEnvironmentVariable("SITE_ADMIN");
            if (!string.IsNullOrWhiteSpace(siteAdmin))
            {
                migrationBuilder.Sql(
                    $"""
                    UPDATE "Users" SET "Role" = 'User'
                    WHERE "UserId" = (
                        SELECT "UserId" FROM "Logins" WHERE "Username" = '{siteAdmin.Replace("'", "''")}'
                    );
                    """);
            }
        }
    }
}
