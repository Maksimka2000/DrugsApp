using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlcoholInteraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAlcoholInteraction",
                table: "ActiveSubstances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ActiveSubstanceAlcoholInteractionOverrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActiveSubstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlcoholSpecialText = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveSubstanceAlcoholInteractionOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveSubstanceAlcoholInteractionOverrides_ActiveSubstances~",
                        column: x => x.ActiveSubstanceId,
                        principalTable: "ActiveSubstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemTexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemTexts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SystemTexts",
                columns: new[] { "Id", "CreatedAtUtc", "Key", "UpdatedAtUtc", "Value" },
                values: new object[] { new Guid("f6c3b9ba-0c43-4f6a-9f42-1c6d7f2b6d1f"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "activeSubstance.alcohol.defaultText", null, "Default alcohol interaction text." });

            migrationBuilder.CreateIndex(
                name: "Index_ActiveSubstances_HasAlcoholInteraction",
                table: "ActiveSubstances",
                column: "HasAlcoholInteraction");

            migrationBuilder.CreateIndex(
                name: "Index_ActiveSubstanceAlcoholInteractionOverrides_ActiveSubstanceId",
                table: "ActiveSubstanceAlcoholInteractionOverrides",
                column: "ActiveSubstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemTexts_Key",
                table: "SystemTexts",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveSubstanceAlcoholInteractionOverrides");

            migrationBuilder.DropTable(
                name: "SystemTexts");

            migrationBuilder.DropIndex(
                name: "Index_ActiveSubstances_HasAlcoholInteraction",
                table: "ActiveSubstances");

            migrationBuilder.DropColumn(
                name: "HasAlcoholInteraction",
                table: "ActiveSubstances");
        }
    }
}
