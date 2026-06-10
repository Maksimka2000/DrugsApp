using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class REGULARDRUG : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_regular_drugs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugFormId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    HistoryReason = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ArchivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_regular_drugs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_client_regular_drugs_DrugForms_DrugFormId",
                        column: x => x.DrugFormId,
                        principalTable: "DrugForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_client_regular_drugs_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "client_regular_drug_intake_times",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegularDrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntakeTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_regular_drug_intake_times", x => x.Id);
                    table.ForeignKey(
                        name: "FK_client_regular_drug_intake_times_client_regular_drugs_Regul~",
                        column: x => x.RegularDrugId,
                        principalTable: "client_regular_drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_client_regular_drug_intake_times_regular_drug_id_intake_time",
                table: "client_regular_drug_intake_times",
                columns: new[] { "RegularDrugId", "IntakeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_drugs_DrugFormId",
                table: "client_regular_drugs",
                column: "DrugFormId");

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_drugs_DrugId",
                table: "client_regular_drugs",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_drugs_status_end_date_live",
                table: "client_regular_drugs",
                columns: new[] { "Status", "EndDate" },
                filter: "\"Status\" IN ('Scheduled', 'Active')");

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_drugs_user_id_archived_at_utc",
                table: "client_regular_drugs",
                columns: new[] { "UserId", "ArchivedAtUtc" },
                filter: "\"Status\" = 'Expired'");

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_drugs_user_id_status",
                table: "client_regular_drugs",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_client_regular_drugs_user_id_drug_id_live",
                table: "client_regular_drugs",
                columns: new[] { "UserId", "DrugId" },
                unique: true,
                filter: "\"Status\" IN ('Scheduled', 'Active')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_regular_drug_intake_times");

            migrationBuilder.DropTable(
                name: "client_regular_drugs");
        }
    }
}
