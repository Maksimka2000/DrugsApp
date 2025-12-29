using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOnSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TempDrugs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "PendingReview",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "FoodIntakeType",
                table: "OralMedicationIntakeGuidelines",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "Sublingual",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "TypeOfDrug",
                table: "Drugs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "Standard",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "TempDrugs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldDefaultValue: "PendingReview");

            migrationBuilder.AlterColumn<string>(
                name: "FoodIntakeType",
                table: "OralMedicationIntakeGuidelines",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldDefaultValue: "Sublingual");

            migrationBuilder.AlterColumn<string>(
                name: "TypeOfDrug",
                table: "Drugs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldDefaultValue: "Standard");
        }
    }
}
