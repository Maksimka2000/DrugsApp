using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillFaqPolishTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO "FaqEntryTranslations" (
                    "Id",
                    "FaqEntryId",
                    "LanguageCode",
                    "Question",
                    "NormalizedQuestion",
                    "Answer",
                    "CreatedAtUtc",
                    "UpdatedAtUtc")
                SELECT
                    gen_random_uuid(),
                    english."FaqEntryId",
                    'pl',
                    english."Question",
                    english."NormalizedQuestion",
                    english."Answer",
                    english."CreatedAtUtc",
                    english."UpdatedAtUtc"
                FROM "FaqEntryTranslations" english
                WHERE english."LanguageCode" = 'en'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "FaqEntryTranslations" polish
                      WHERE polish."FaqEntryId" = english."FaqEntryId"
                        AND polish."LanguageCode" = 'pl');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "FaqEntryTranslations" polish
                USING "FaqEntryTranslations" english
                WHERE polish."FaqEntryId" = english."FaqEntryId"
                  AND polish."LanguageCode" = 'pl'
                  AND english."LanguageCode" = 'en'
                  AND polish."Question" = english."Question"
                  AND polish."Answer" = english."Answer";
                """);
        }
    }
}
