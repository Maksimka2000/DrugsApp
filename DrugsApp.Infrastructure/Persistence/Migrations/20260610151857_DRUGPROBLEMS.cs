using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DRUGPROBLEMS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DrugProblems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomDrugName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProblemNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugProblems", x => x.Id);
                    table.CheckConstraint("CK_DrugProblems_SourceXor", "(CASE WHEN \"DrugId\" IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN \"CustomDrugName\" IS NOT NULL THEN 1 ELSE 0 END) = 1");
                    table.ForeignKey(
                        name: "FK_DrugProblems_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrugProblemTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProblemTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugProblemTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrugProblems_DrugId",
                table: "DrugProblems",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugProblems_UserId",
                table: "DrugProblems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugProblems_UserId_CreatedAtUtc_Id",
                table: "DrugProblems",
                columns: new[] { "UserId", "CreatedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "UX_DrugProblems_UserId_CustomDrugName",
                table: "DrugProblems",
                columns: new[] { "UserId", "CustomDrugName" },
                unique: true,
                filter: "\"CustomDrugName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_DrugProblems_UserId_DrugId",
                table: "DrugProblems",
                columns: new[] { "UserId", "DrugId" },
                unique: true,
                filter: "\"DrugId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DrugProblemTypes_ProblemTypeName_Id",
                table: "DrugProblemTypes",
                columns: new[] { "ProblemTypeName", "Id" });

            migrationBuilder.CreateIndex(
                name: "UX_DrugProblemTypes_ProblemTypeName_Lower",
                table: "DrugProblemTypes",
                column: "ProblemTypeName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrugProblems");

            migrationBuilder.DropTable(
                name: "DrugProblemTypes");
        }
    }
}
