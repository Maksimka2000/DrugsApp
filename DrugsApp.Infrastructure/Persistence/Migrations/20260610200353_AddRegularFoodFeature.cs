using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegularFoodFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_own_supplements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_own_supplements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "client_regular_foods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_client_regular_foods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_client_regular_foods_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "client_own_supplement_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnSupplementId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplementId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_own_supplement_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_client_own_supplement_items_Supplements_SupplementId",
                        column: x => x.SupplementId,
                        principalTable: "Supplements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_client_own_supplement_items_client_own_supplements_OwnSuppl~",
                        column: x => x.OwnSupplementId,
                        principalTable: "client_own_supplements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "client_regular_supplements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SupplementId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnSupplementId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_client_regular_supplements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_client_regular_supplements_Supplements_SupplementId",
                        column: x => x.SupplementId,
                        principalTable: "Supplements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_client_regular_supplements_client_own_supplements_OwnSupple~",
                        column: x => x.OwnSupplementId,
                        principalTable: "client_own_supplements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "client_regular_food_intake_times",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegularFoodId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntakeTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_regular_food_intake_times", x => x.Id);
                    table.ForeignKey(
                        name: "FK_client_regular_food_intake_times_client_regular_foods_Regul~",
                        column: x => x.RegularFoodId,
                        principalTable: "client_regular_foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "client_regular_supplement_intake_times",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegularSupplementId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntakeTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_regular_supplement_intake_times", x => x.Id);
                    table.ForeignKey(
                        name: "FK_client_regular_supplement_intake_times_client_regular_suppl~",
                        column: x => x.RegularSupplementId,
                        principalTable: "client_regular_supplements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_client_own_supplement_items_SupplementId",
                table: "client_own_supplement_items",
                column: "SupplementId");

            migrationBuilder.CreateIndex(
                name: "UX_client_own_supplement_items_own_supplement_id_sort_order",
                table: "client_own_supplement_items",
                columns: new[] { "OwnSupplementId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_client_own_supplement_items_own_supplement_id_supplement_id",
                table: "client_own_supplement_items",
                columns: new[] { "OwnSupplementId", "SupplementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_client_own_supplements_user_id_normalized_name_id",
                table: "client_own_supplements",
                columns: new[] { "UserId", "NormalizedName", "Id" });

            migrationBuilder.CreateIndex(
                name: "UX_client_own_supplements_user_id_normalized_name",
                table: "client_own_supplements",
                columns: new[] { "UserId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_client_regular_food_intake_times_regular_food_id_intake_time",
                table: "client_regular_food_intake_times",
                columns: new[] { "RegularFoodId", "IntakeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_foods_FoodId",
                table: "client_regular_foods",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_foods_user_id_archived_at_utc",
                table: "client_regular_foods",
                columns: new[] { "UserId", "ArchivedAtUtc" },
                filter: "\"Status\" = 'Expired'");

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_foods_user_id_status",
                table: "client_regular_foods",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_foods_user_id_status_end_date",
                table: "client_regular_foods",
                columns: new[] { "UserId", "Status", "EndDate" },
                filter: "\"Status\" IN ('Scheduled', 'Active')");

            migrationBuilder.CreateIndex(
                name: "UX_client_regular_foods_user_id_food_id_live",
                table: "client_regular_foods",
                columns: new[] { "UserId", "FoodId" },
                unique: true,
                filter: "\"Status\" IN ('Scheduled', 'Active')");

            migrationBuilder.CreateIndex(
                name: "UX_client_regular_supplement_intake_times_regular_supplement_id_intake_time",
                table: "client_regular_supplement_intake_times",
                columns: new[] { "RegularSupplementId", "IntakeTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_supplements_OwnSupplementId",
                table: "client_regular_supplements",
                column: "OwnSupplementId");

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_supplements_status_end_date",
                table: "client_regular_supplements",
                columns: new[] { "Status", "EndDate" },
                filter: "\"Status\" IN ('Scheduled', 'Active') AND \"EndDate\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_supplements_SupplementId",
                table: "client_regular_supplements",
                column: "SupplementId");

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_supplements_user_id_archived_at_utc",
                table: "client_regular_supplements",
                columns: new[] { "UserId", "ArchivedAtUtc" },
                filter: "\"Status\" = 'Expired'");

            migrationBuilder.CreateIndex(
                name: "IX_client_regular_supplements_user_id_status",
                table: "client_regular_supplements",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_client_regular_supplements_user_id_own_supplement_id_live",
                table: "client_regular_supplements",
                columns: new[] { "UserId", "OwnSupplementId" },
                unique: true,
                filter: "\"Status\" IN ('Scheduled', 'Active') AND \"Source\" = 'Own'");

            migrationBuilder.CreateIndex(
                name: "UX_client_regular_supplements_user_id_supplement_id_live",
                table: "client_regular_supplements",
                columns: new[] { "UserId", "SupplementId" },
                unique: true,
                filter: "\"Status\" IN ('Scheduled', 'Active') AND \"Source\" = 'Database'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_own_supplement_items");

            migrationBuilder.DropTable(
                name: "client_regular_food_intake_times");

            migrationBuilder.DropTable(
                name: "client_regular_supplement_intake_times");

            migrationBuilder.DropTable(
                name: "client_regular_foods");

            migrationBuilder.DropTable(
                name: "client_regular_supplements");

            migrationBuilder.DropTable(
                name: "client_own_supplements");
        }
    }
}
