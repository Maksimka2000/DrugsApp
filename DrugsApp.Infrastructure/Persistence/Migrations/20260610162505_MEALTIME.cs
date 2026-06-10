using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MEALTIME : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientMealTimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NormalizedMealName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IntakeTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientMealTimes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientMealTimes_UserId",
                table: "ClientMealTimes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientMealTimes_UserId_IntakeTime",
                table: "ClientMealTimes",
                columns: new[] { "UserId", "IntakeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientMealTimes_UserId_NormalizedMealName",
                table: "ClientMealTimes",
                columns: new[] { "UserId", "NormalizedMealName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientMealTimes");
        }
    }
}
