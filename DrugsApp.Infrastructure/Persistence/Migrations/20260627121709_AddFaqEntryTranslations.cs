using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFaqEntryTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaqEntryTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Question = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    NormalizedQuestion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    FaqEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqEntryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaqEntryTranslations_FaqEntries_FaqEntryId",
                        column: x => x.FaqEntryId,
                        principalTable: "FaqEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    "Id",
                    'en',
                    "Question",
                    "NormalizedQuestion",
                    "Answer",
                    "CreatedAtUtc",
                    "UpdatedAtUtc"
                FROM "FaqEntries";
                """);

            migrationBuilder.DropIndex(
                name: "Index_FaqEntries_NormalizedQuestion",
                table: "FaqEntries");

            migrationBuilder.DropIndex(
                name: "Index_FaqEntries_NormalizedQuestion_trgm",
                table: "FaqEntries");

            migrationBuilder.DropIndex(
                name: "Index_FaqEntries_Question_CreatedAtUtc_Id",
                table: "FaqEntries");

            migrationBuilder.DropColumn(
                name: "Answer",
                table: "FaqEntries");

            migrationBuilder.DropColumn(
                name: "NormalizedQuestion",
                table: "FaqEntries");

            migrationBuilder.DropColumn(
                name: "Question",
                table: "FaqEntries");

            migrationBuilder.CreateIndex(
                name: "Index_FaqEntryTranslations_FaqEntryId_LanguageCode",
                table: "FaqEntryTranslations",
                columns: new[] { "FaqEntryId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_FaqEntryTranslations_LanguageCode_NormalizedQuestion",
                table: "FaqEntryTranslations",
                columns: new[] { "LanguageCode", "NormalizedQuestion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_FaqEntryTranslations_NormalizedQuestion_trgm",
                table: "FaqEntryTranslations",
                column: "NormalizedQuestion")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "Index_FaqEntryTranslations_Question_CreatedAtUtc_Id",
                table: "FaqEntryTranslations",
                columns: new[] { "Question", "CreatedAtUtc", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaqEntryTranslations");

            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "FaqEntries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedQuestion",
                table: "FaqEntries",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Question",
                table: "FaqEntries",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "Index_FaqEntries_NormalizedQuestion",
                table: "FaqEntries",
                column: "NormalizedQuestion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_FaqEntries_NormalizedQuestion_trgm",
                table: "FaqEntries",
                column: "NormalizedQuestion")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "Index_FaqEntries_Question_CreatedAtUtc_Id",
                table: "FaqEntries",
                columns: new[] { "Question", "CreatedAtUtc", "Id" });
        }
    }
}
