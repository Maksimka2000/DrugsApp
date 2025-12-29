using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewFeatureTempDrug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TempDrugs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CommercialName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ActiveSubstance = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TypeOfProduct = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RouteOfAdministration = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PharmaceuticalFormulation = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ATCCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Leaflet = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EducationalMaterials = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Dosage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LinkedDrugFormId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedDrugVetId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedImportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempDrugs", x => x.Id);
                    table.CheckConstraint("CK_TempDrugs_LinkedDrugIds_Xor", "\"LinkedDrugFormId\" IS NULL OR \"LinkedDrugVetId\" IS NULL");
                });

            migrationBuilder.CreateIndex(
                name: "Index_TempDrugs_CommercialName_trgm",
                table: "TempDrugs",
                column: "CommercialName")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "Index_TempDrugs_LinkedDrugFormId",
                table: "TempDrugs",
                column: "LinkedDrugFormId");

            migrationBuilder.CreateIndex(
                name: "Index_TempDrugs_LinkedDrugVetId",
                table: "TempDrugs",
                column: "LinkedDrugVetId");

            migrationBuilder.CreateIndex(
                name: "Index_TempDrugs_LinkedImportId",
                table: "TempDrugs",
                column: "LinkedImportId");

            migrationBuilder.CreateIndex(
                name: "Index_TempDrugs_ProductIdentifier",
                table: "TempDrugs",
                column: "ProductIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_TempDrugs_Status",
                table: "TempDrugs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "Index_TempDrugs_TypeOfProduct",
                table: "TempDrugs",
                column: "TypeOfProduct");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TempDrugs");
        }
    }
}
