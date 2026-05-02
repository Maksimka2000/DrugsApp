using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SuplementGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActiveSubstanceGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveSubstanceGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Foods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SearchText = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Aliases = table.Column<List<string>>(type: "jsonb", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplementGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplementGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Supplements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SearchText = table.Column<string>(type: "text", nullable: false),
                    Aliases = table.Column<IReadOnlyCollection<string>>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActiveSubstanceGroupMembers",
                columns: table => new
                {
                    ActiveSubstanceGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActiveSubstanceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveSubstanceGroupMembers", x => new { x.ActiveSubstanceGroupId, x.ActiveSubstanceId });
                    table.ForeignKey(
                        name: "FK_ActiveSubstanceGroupMembers_ActiveSubstanceGroups_ActiveSub~",
                        column: x => x.ActiveSubstanceGroupId,
                        principalTable: "ActiveSubstanceGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActiveSubstanceGroupMembers_ActiveSubstances_ActiveSubstanc~",
                        column: x => x.ActiveSubstanceId,
                        principalTable: "ActiveSubstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodGroupFoods",
                columns: table => new
                {
                    FoodGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodGroupFoods", x => new { x.FoodGroupId, x.FoodId });
                    table.ForeignKey(
                        name: "FK_FoodGroupFoods_FoodGroups_FoodGroupId",
                        column: x => x.FoodGroupId,
                        principalTable: "FoodGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodGroupFoods_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplementGroupSupplements",
                columns: table => new
                {
                    SupplementGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplementId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplementGroupSupplements", x => new { x.SupplementGroupId, x.SupplementId });
                    table.ForeignKey(
                        name: "FK_SupplementGroupSupplements_SupplementGroups_SupplementGroup~",
                        column: x => x.SupplementGroupId,
                        principalTable: "SupplementGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplementGroupSupplements_Supplements_SupplementId",
                        column: x => x.SupplementId,
                        principalTable: "Supplements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveSubstanceGroupMembers_ActiveSubstanceId_ActiveSubstan~",
                table: "ActiveSubstanceGroupMembers",
                columns: new[] { "ActiveSubstanceId", "ActiveSubstanceGroupId" });

            migrationBuilder.CreateIndex(
                name: "Index_ActiveSubstanceGroups_Name",
                table: "ActiveSubstanceGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_ActiveSubstanceGroups_Name_trgm",
                table: "ActiveSubstanceGroups",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_FoodGroupFoods_FoodId_FoodGroupId",
                table: "FoodGroupFoods",
                columns: new[] { "FoodId", "FoodGroupId" });

            migrationBuilder.CreateIndex(
                name: "Index_FoodGroups_Name",
                table: "FoodGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_FoodGroups_Name_trgm",
                table: "FoodGroups",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "Index_Foods_Name",
                table: "Foods",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Index_Foods_SearchText_trgm",
                table: "Foods",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplementGroups_Name",
                table: "SupplementGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplementGroupSupplements_SupplementId_SupplementGroupId",
                table: "SupplementGroupSupplements",
                columns: new[] { "SupplementId", "SupplementGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_Supplements_Name",
                table: "Supplements",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Supplements_SearchText_trgm",
                table: "Supplements",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveSubstanceGroupMembers");

            migrationBuilder.DropTable(
                name: "FoodGroupFoods");

            migrationBuilder.DropTable(
                name: "SupplementGroupSupplements");

            migrationBuilder.DropTable(
                name: "ActiveSubstanceGroups");

            migrationBuilder.DropTable(
                name: "FoodGroups");

            migrationBuilder.DropTable(
                name: "Foods");

            migrationBuilder.DropTable(
                name: "SupplementGroups");

            migrationBuilder.DropTable(
                name: "Supplements");
        }
    }
}
