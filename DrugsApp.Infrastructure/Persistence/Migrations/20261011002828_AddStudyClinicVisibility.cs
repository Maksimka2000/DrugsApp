using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyClinicVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "visibility",
                table: "study_clinics",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Public");

            migrationBuilder.CreateIndex(
                name: "ix_study_clinics_created_by_visibility",
                table: "study_clinics",
                columns: new[] { "created_by_user_id", "visibility" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_study_clinics_created_by_visibility",
                table: "study_clinics");

            migrationBuilder.DropColumn(
                name: "visibility",
                table: "study_clinics");
        }
    }
}
