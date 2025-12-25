using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewFeatureActiveSubstance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "ActiveSubstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActiveSubstanceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveSubstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OralMedicationIntakeGuidelines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActiveSubstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodIntakeType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    WaitBeforeMinutes = table.Column<int>(type: "integer", nullable: true),
                    WaitAfterMinutes = table.Column<int>(type: "integer", nullable: true),
                    MealNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AvoidFoodDrug = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AvoidFoodTherapy = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LiquidNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LiquidInstructions = table.Column<bool>(type: "boolean", nullable: true),
                    RecommendedMorning = table.Column<bool>(type: "boolean", nullable: true),
                    RecommendedEvening = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OralMedicationIntakeGuidelines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OralMedicationIntakeGuidelines_ActiveSubstances_ActiveSubst~",
                        column: x => x.ActiveSubstanceId,
                        principalTable: "ActiveSubstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "Index_ActiveSubstances_ActiveSubstanceName",
                table: "ActiveSubstances",
                column: "ActiveSubstanceName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_ActiveSubstances_ActiveSubstanceName_trgm",
                table: "ActiveSubstances",
                column: "ActiveSubstanceName")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_OralMedicationIntakeGuidelines_ActiveSubstanceId",
                table: "OralMedicationIntakeGuidelines",
                column: "ActiveSubstanceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OralMedicationIntakeGuidelines");

            migrationBuilder.DropTable(
                name: "ActiveSubstances");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
