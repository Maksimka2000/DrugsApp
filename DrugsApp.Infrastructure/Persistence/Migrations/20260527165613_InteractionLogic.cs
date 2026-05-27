using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InteractionLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InteractionSeverities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeverityNumber = table.Column<short>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionSeverities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Interactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultText = table.Column<string>(type: "text", nullable: true),
                    StomachText = table.Column<string>(type: "text", nullable: true),
                    InteractionSeverityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interactions_InteractionSeverities_InteractionSeverityId",
                        column: x => x.InteractionSeverityId,
                        principalTable: "InteractionSeverities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InteractionExpanded",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InteractionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartyTypeA = table.Column<string>(type: "text", nullable: false),
                    PartyIdA = table.Column<Guid>(type: "uuid", nullable: false),
                    PartyTypeB = table.Column<string>(type: "text", nullable: false),
                    PartyIdB = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionExpanded", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractionExpanded_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InteractionParties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InteractionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartyType = table.Column<string>(type: "text", nullable: false),
                    ActiveSubstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActiveSubstanceGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    FoodId = table.Column<Guid>(type: "uuid", nullable: true),
                    FoodGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplementId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplementGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionParties", x => x.Id);
                    table.CheckConstraint("CK_InteractionParties_ExactlyOneFk", "num_nonnulls(\"ActiveSubstanceId\",\"ActiveSubstanceGroupId\",\"FoodId\",\"FoodGroupId\",\"SupplementId\",\"SupplementGroupId\") = 1");
                    table.ForeignKey(
                        name: "FK_InteractionParties_Interactions_InteractionId",
                        column: x => x.InteractionId,
                        principalTable: "Interactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "InteractionSeverities",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "Name", "SeverityNumber", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111110111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Major", (short)1, null },
                    { new Guid("22222222-2222-2222-2222-222222221222"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Moderate", (short)2, null },
                    { new Guid("33333333-3333-3333-3333-333333332333"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Minor", (short)3, null },
                    { new Guid("44444444-4444-4444-4444-444444443444"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Not applicable", (short)4, null }
                });

            migrationBuilder.CreateIndex(
                name: "Index_InteractionExpanded_PartyA",
                table: "InteractionExpanded",
                columns: new[] { "PartyTypeA", "PartyIdA" });

            migrationBuilder.CreateIndex(
                name: "Index_InteractionExpanded_PartyB",
                table: "InteractionExpanded",
                columns: new[] { "PartyTypeB", "PartyIdB" });

            migrationBuilder.CreateIndex(
                name: "UX_InteractionExpanded_UniquePair",
                table: "InteractionExpanded",
                columns: new[] { "InteractionId", "PartyTypeA", "PartyIdA", "PartyTypeB", "PartyIdB" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_InteractionParties_ActiveSubstanceGroupId",
                table: "InteractionParties",
                column: "ActiveSubstanceGroupId");

            migrationBuilder.CreateIndex(
                name: "Index_InteractionParties_ActiveSubstanceId",
                table: "InteractionParties",
                column: "ActiveSubstanceId");

            migrationBuilder.CreateIndex(
                name: "Index_InteractionParties_FoodGroupId",
                table: "InteractionParties",
                column: "FoodGroupId");

            migrationBuilder.CreateIndex(
                name: "Index_InteractionParties_FoodId",
                table: "InteractionParties",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "Index_InteractionParties_InteractionId",
                table: "InteractionParties",
                column: "InteractionId");

            migrationBuilder.CreateIndex(
                name: "Index_InteractionParties_SupplementGroupId",
                table: "InteractionParties",
                column: "SupplementGroupId");

            migrationBuilder.CreateIndex(
                name: "Index_InteractionParties_SupplementId",
                table: "InteractionParties",
                column: "SupplementId");

            migrationBuilder.CreateIndex(
                name: "UX_InteractionParties_Food",
                table: "InteractionParties",
                columns: new[] { "InteractionId", "FoodId" },
                unique: true,
                filter: "\"PartyType\" = 'Food'");

            migrationBuilder.CreateIndex(
                name: "UX_InteractionParties_FoodGroup",
                table: "InteractionParties",
                columns: new[] { "InteractionId", "FoodGroupId" },
                unique: true,
                filter: "\"PartyType\" = 'FoodGroup'");

            migrationBuilder.CreateIndex(
                name: "UX_InteractionParties_Substance",
                table: "InteractionParties",
                columns: new[] { "InteractionId", "ActiveSubstanceId" },
                unique: true,
                filter: "\"PartyType\" = 'Substance'");

            migrationBuilder.CreateIndex(
                name: "UX_InteractionParties_SubstanceGroup",
                table: "InteractionParties",
                columns: new[] { "InteractionId", "ActiveSubstanceGroupId" },
                unique: true,
                filter: "\"PartyType\" = 'SubstanceGroup'");

            migrationBuilder.CreateIndex(
                name: "UX_InteractionParties_Supplement",
                table: "InteractionParties",
                columns: new[] { "InteractionId", "SupplementId" },
                unique: true,
                filter: "\"PartyType\" = 'Supplement'");

            migrationBuilder.CreateIndex(
                name: "UX_InteractionParties_SupplementGroup",
                table: "InteractionParties",
                columns: new[] { "InteractionId", "SupplementGroupId" },
                unique: true,
                filter: "\"PartyType\" = 'SupplementGroup'");

            migrationBuilder.CreateIndex(
                name: "Index_Interactions_CreatedAtUtc",
                table: "Interactions",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "Index_Interactions_InteractionSeverityId",
                table: "Interactions",
                column: "InteractionSeverityId");

            migrationBuilder.CreateIndex(
                name: "Index_Interactions_UpdatedAtUtc",
                table: "Interactions",
                column: "UpdatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "Index_InteractionSeverities_Name",
                table: "InteractionSeverities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_InteractionSeverities_SeverityNumber",
                table: "InteractionSeverities",
                column: "SeverityNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InteractionExpanded");

            migrationBuilder.DropTable(
                name: "InteractionParties");

            migrationBuilder.DropTable(
                name: "Interactions");

            migrationBuilder.DropTable(
                name: "InteractionSeverities");
        }
    }
}
