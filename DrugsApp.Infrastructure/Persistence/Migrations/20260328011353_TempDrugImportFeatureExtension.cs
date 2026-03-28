using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TempDrugImportFeatureExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "TempDrugs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "DefaultMappingForColumnsAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceColumnName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    MappedFieldName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultMappingForColumnsAttributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TempDrugImportBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HangfireJobId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ImportedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NameOfFile = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    BlobFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImportFinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourcePath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    MappingPlanJson = table.Column<string>(type: "text", nullable: true),
                    TotalQuantityCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ImportedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ImportedIds = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    SkippedCounter = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SkippedIds = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    FailedToCreate = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    RecordsDeletedInRevert = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RecordsDeletedInRevertIds = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    FailedToDelete = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    StatusOfImport = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ErrorNote = table.Column<string>(type: "text", nullable: true),
                    ScheduledInfo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempDrugImportBatches", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DefaultMappingForColumnsAttributes",
                columns: new[] { "Id", "CreatedAtUtc", "MappedFieldName", "SourceColumnName", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("00dbb8b7-30d5-421f-96a2-2d9920e786ce"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "EducationalMaterials", "Materiały edukacyjne dla osoby wykonującej zawód medyczny", null },
                    { new Guid("05d450e8-2c20-4606-913a-5c44a8ca7dc4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "TypeOfProduct", "Rodzaj preparatu", null },
                    { new Guid("11f856d0-ca3a-4b95-9c07-9c8ee10473a1"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ProductIdentifier", "Identyfikator Produktu Leczniczego", null },
                    { new Guid("15c2fea5-0f41-40af-9179-f6dd7b06a04c"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ActiveSubstance", "Nazwa powszechnie stosowana", null },
                    { new Guid("2d03f41a-0a99-49da-bbc6-9c56a9226156"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ATCCode", "Kod ATC", null },
                    { new Guid("35aecb0c-29ea-43ea-9a53-6608a7991ac4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RouteOfAdministration", "Droga podania / Gatunek / Tkanka / Okres karencji", null },
                    { new Guid("7290f113-d8cb-4d3a-b6ab-ef866fa69a17"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Leaflet", "Ulotka", null },
                    { new Guid("8cfd62cc-ebb3-4409-933d-ba227a598e4a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PharmaceuticalFormulation", "Postać farmaceutyczna", null },
                    { new Guid("a26ab581-877b-490a-93f8-3e9234e36a13"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CommercialName", "Nazwa Produktu Leczniczego", null },
                    { new Guid("a8437908-8f3a-48d0-b4e8-4d5f35ef90b5"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dosage", "Moc", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefaultMappingForColumnsAttributes_SourceColumnName",
                table: "DefaultMappingForColumnsAttributes",
                column: "SourceColumnName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TempDrugImportBatches_CreatedAtUtc",
                table: "TempDrugImportBatches",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TempDrugImportBatches_StatusOfImport",
                table: "TempDrugImportBatches",
                column: "StatusOfImport");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultMappingForColumnsAttributes");

            migrationBuilder.DropTable(
                name: "TempDrugImportBatches");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "TempDrugs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
