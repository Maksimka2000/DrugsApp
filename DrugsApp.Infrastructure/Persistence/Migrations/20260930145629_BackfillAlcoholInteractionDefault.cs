using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillAlcoholInteractionDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "ActiveSubstances" substance
                SET "HasAlcoholInteraction" = TRUE
                WHERE substance."HasAlcoholInteraction" = FALSE
                  AND EXISTS (
                      SELECT 1
                      FROM "ActiveSubstanceAlcoholInteractionOverrides" alcoholOverride
                      WHERE alcoholOverride."ActiveSubstanceId" = substance."Id"
                        AND BTRIM(alcoholOverride."AlcoholSpecialText") <> '');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
