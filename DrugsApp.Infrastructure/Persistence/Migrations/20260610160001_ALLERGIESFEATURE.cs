using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ALLERGIESFEATURE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientAllergySummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SevereReactionStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CarriesAllergyMedication = table.Column<bool>(type: "boolean", nullable: true),
                    PollenDustMitesAllergyStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    InsectVenomAllergyStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    LatexAllergyStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAllergySummaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientDrugAllergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomDrugName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CustomDrugNameNormalized = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientDrugAllergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientDrugAllergies_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientFoodAllergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomFoodName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CustomFoodNameNormalized = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientFoodAllergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientFoodAllergies_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientOtherAllergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllergyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AllergyNameNormalized = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientOtherAllergies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientSupplementAllergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplementId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomSupplementName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CustomSupplementNameNormalized = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSupplementAllergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientSupplementAllergies_Supplements_SupplementId",
                        column: x => x.SupplementId,
                        principalTable: "Supplements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientAllergySummaries_UserId",
                table: "ClientAllergySummaries",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientDrugAllergies_DrugId",
                table: "ClientDrugAllergies",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDrugAllergies_UserId",
                table: "ClientDrugAllergies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDrugAllergies_UserId_CustomDrugNameNormalized_Custom",
                table: "ClientDrugAllergies",
                columns: new[] { "UserId", "CustomDrugNameNormalized" },
                unique: true,
                filter: "\"CustomDrugNameNormalized\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDrugAllergies_UserId_DrugId_Database",
                table: "ClientDrugAllergies",
                columns: new[] { "UserId", "DrugId" },
                unique: true,
                filter: "\"DrugId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDrugAllergies_UserId_Source_Id",
                table: "ClientDrugAllergies",
                columns: new[] { "UserId", "Source", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientFoodAllergies_FoodId",
                table: "ClientFoodAllergies",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientFoodAllergies_UserId",
                table: "ClientFoodAllergies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientFoodAllergies_UserId_CustomFoodNameNormalized_Custom",
                table: "ClientFoodAllergies",
                columns: new[] { "UserId", "CustomFoodNameNormalized" },
                unique: true,
                filter: "\"CustomFoodNameNormalized\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClientFoodAllergies_UserId_FoodId_Database",
                table: "ClientFoodAllergies",
                columns: new[] { "UserId", "FoodId" },
                unique: true,
                filter: "\"FoodId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClientOtherAllergies_UserId",
                table: "ClientOtherAllergies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientOtherAllergies_UserId_AllergyNameNormalized",
                table: "ClientOtherAllergies",
                columns: new[] { "UserId", "AllergyNameNormalized" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientSupplementAllergies_SupplementId",
                table: "ClientSupplementAllergies",
                column: "SupplementId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSupplementAllergies_UserId",
                table: "ClientSupplementAllergies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSupplementAllergies_UserId_CustomSupplementNameNormalized_Custom",
                table: "ClientSupplementAllergies",
                columns: new[] { "UserId", "CustomSupplementNameNormalized" },
                unique: true,
                filter: "\"CustomSupplementNameNormalized\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSupplementAllergies_UserId_SupplementId_Database",
                table: "ClientSupplementAllergies",
                columns: new[] { "UserId", "SupplementId" },
                unique: true,
                filter: "\"SupplementId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientAllergySummaries");

            migrationBuilder.DropTable(
                name: "ClientDrugAllergies");

            migrationBuilder.DropTable(
                name: "ClientFoodAllergies");

            migrationBuilder.DropTable(
                name: "ClientOtherAllergies");

            migrationBuilder.DropTable(
                name: "ClientSupplementAllergies");
        }
    }
}
