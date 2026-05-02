using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixOnHangfire3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecordsDeletedInRevert",
                table: "TempDrugImportBatches",
                newName: "RecordsDeletedInRevertCounter");

            migrationBuilder.AddColumn<int>(
                name: "FailedToCreateCounter",
                table: "TempDrugImportBatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FailedToDeleteCounter",
                table: "TempDrugImportBatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedToCreateCounter",
                table: "TempDrugImportBatches");

            migrationBuilder.DropColumn(
                name: "FailedToDeleteCounter",
                table: "TempDrugImportBatches");

            migrationBuilder.RenameColumn(
                name: "RecordsDeletedInRevertCounter",
                table: "TempDrugImportBatches",
                newName: "RecordsDeletedInRevert");
        }
    }
}
