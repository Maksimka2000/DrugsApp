using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewFeatureDrug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_RoutesOfAdministration_RouteOfAdministrationName",
                table: "RoutesOfAdministration",
                newName: "Index_RoutesOfAdministration_RouteOfAdministrationName");

            migrationBuilder.RenameIndex(
                name: "IX_PharmaceuticalForms_FormulationName",
                table: "PharmaceuticalForms",
                newName: "Index_PharmaceuticalForms_FormulationName");

            migrationBuilder.AddColumn<bool>(
                name: "IsOralIntake",
                table: "RoutesOfAdministration",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Drugs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommercialName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TypeOfProduct = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TypeOfDrug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DrugFormCounter = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drugs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugActiveSubstances",
                columns: table => new
                {
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActiveSubstanceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugActiveSubstances", x => new { x.DrugId, x.ActiveSubstanceId });
                    table.ForeignKey(
                        name: "FK_DrugActiveSubstances_ActiveSubstances_ActiveSubstanceId",
                        column: x => x.ActiveSubstanceId,
                        principalTable: "ActiveSubstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugActiveSubstances_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductIdentifier = table.Column<int>(type: "integer", nullable: false),
                    AtcCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Dosage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Leaflet = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EducationMaterials = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsOral = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrugForms_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugFormPharmaceuticalForms",
                columns: table => new
                {
                    DrugFormId = table.Column<Guid>(type: "uuid", nullable: false),
                    PharmaceuticalFormId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugFormPharmaceuticalForms", x => new { x.DrugFormId, x.PharmaceuticalFormId });
                    table.ForeignKey(
                        name: "FK_DrugFormPharmaceuticalForms_DrugForms_DrugFormId",
                        column: x => x.DrugFormId,
                        principalTable: "DrugForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugFormPharmaceuticalForms_PharmaceuticalForms_Pharmaceuti~",
                        column: x => x.PharmaceuticalFormId,
                        principalTable: "PharmaceuticalForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrugFormRoutesOfAdministration",
                columns: table => new
                {
                    DrugFormId = table.Column<Guid>(type: "uuid", nullable: false),
                    RouteOfAdministrationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugFormRoutesOfAdministration", x => new { x.DrugFormId, x.RouteOfAdministrationId });
                    table.ForeignKey(
                        name: "FK_DrugFormRoutesOfAdministration_DrugForms_DrugFormId",
                        column: x => x.DrugFormId,
                        principalTable: "DrugForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrugFormRoutesOfAdministration_RoutesOfAdministration_Route~",
                        column: x => x.RouteOfAdministrationId,
                        principalTable: "RoutesOfAdministration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "Index_RoutesOfAdministration_RouteOfAdministrationName_trgm",
                table: "RoutesOfAdministration",
                column: "RouteOfAdministrationName")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "Index_PharmaceuticalForms_FormulationName_trgm",
                table: "PharmaceuticalForms",
                column: "FormulationName")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "Index_DrugActiveSubstances_ActiveSubstanceId",
                table: "DrugActiveSubstances",
                column: "ActiveSubstanceId");

            migrationBuilder.CreateIndex(
                name: "Index_DrugFormPharmaceuticalForms_PharmaceuticalFormId",
                table: "DrugFormPharmaceuticalForms",
                column: "PharmaceuticalFormId");

            migrationBuilder.CreateIndex(
                name: "Index_DrugFormRoutesOfAdministration_RouteOfAdministrationId",
                table: "DrugFormRoutesOfAdministration",
                column: "RouteOfAdministrationId");

            migrationBuilder.CreateIndex(
                name: "Index_DrugForms_AtcCode",
                table: "DrugForms",
                column: "AtcCode");

            migrationBuilder.CreateIndex(
                name: "Index_DrugForms_AtcCode_trgm",
                table: "DrugForms",
                column: "AtcCode")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "Index_DrugForms_DrugId",
                table: "DrugForms",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "Index_DrugForms_ProductIdentifier",
                table: "DrugForms",
                column: "ProductIdentifier");

            migrationBuilder.CreateIndex(
                name: "Index_Drugs_CommercialName",
                table: "Drugs",
                column: "CommercialName");

            migrationBuilder.CreateIndex(
                name: "Index_Drugs_CommercialName_Id",
                table: "Drugs",
                columns: new[] { "CommercialName", "Id" });

            migrationBuilder.CreateIndex(
                name: "Index_Drugs_CommercialName_trgm",
                table: "Drugs",
                column: "CommercialName")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrugActiveSubstances");

            migrationBuilder.DropTable(
                name: "DrugFormPharmaceuticalForms");

            migrationBuilder.DropTable(
                name: "DrugFormRoutesOfAdministration");

            migrationBuilder.DropTable(
                name: "DrugForms");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropIndex(
                name: "Index_RoutesOfAdministration_RouteOfAdministrationName_trgm",
                table: "RoutesOfAdministration");

            migrationBuilder.DropIndex(
                name: "Index_PharmaceuticalForms_FormulationName_trgm",
                table: "PharmaceuticalForms");

            migrationBuilder.DropColumn(
                name: "IsOralIntake",
                table: "RoutesOfAdministration");

            migrationBuilder.RenameIndex(
                name: "Index_RoutesOfAdministration_RouteOfAdministrationName",
                table: "RoutesOfAdministration",
                newName: "IX_RoutesOfAdministration_RouteOfAdministrationName");

            migrationBuilder.RenameIndex(
                name: "Index_PharmaceuticalForms_FormulationName",
                table: "PharmaceuticalForms",
                newName: "IX_PharmaceuticalForms_FormulationName");
        }
    }
}
