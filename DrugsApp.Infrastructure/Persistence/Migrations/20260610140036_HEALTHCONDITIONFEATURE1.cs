using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HEALTHCONDITIONFEATURE1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HealthConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthConditionName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedHealthConditionName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthConditions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pending_email_changes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pending_email_changes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pending_email_changes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProfilePhotoStorageKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserHealthConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthConditionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomHealthConditionName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedCustomHealthConditionName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHealthConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHealthConditions_HealthConditions_HealthConditionId",
                        column: x => x.HealthConditionId,
                        principalTable: "HealthConditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "client_reproductive_health_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentlyPregnant = table.Column<bool>(type: "boolean", nullable: false),
                    PlanningToGetPregnant = table.Column<bool>(type: "boolean", nullable: false),
                    Menopause = table.Column<bool>(type: "boolean", nullable: false),
                    ContraceptionMethodCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_reproductive_health_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_client_reproductive_health_profiles_user_profiles_UserId",
                        column: x => x.UserId,
                        principalTable: "user_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "healthcare_provider_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    OtherProfessionLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WorkplaceLabel = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ShortDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OnboardingCompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_healthcare_provider_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_healthcare_provider_profiles_user_profiles_UserId",
                        column: x => x.UserId,
                        principalTable: "user_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_client_reproductive_health_profiles_UserId",
                table: "client_reproductive_health_profiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_healthcare_provider_profiles_UserId",
                table: "healthcare_provider_profiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthConditions_NormalizedHealthConditionName",
                table: "HealthConditions",
                column: "NormalizedHealthConditionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthConditions_NormalizedHealthConditionName_trgm",
                table: "HealthConditions",
                column: "NormalizedHealthConditionName")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_pending_email_changes_ExpiresAtUtc",
                table: "pending_email_changes",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_pending_email_changes_UserId",
                table: "pending_email_changes",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles_Email",
                table: "user_profiles",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserHealthConditions_HealthConditionId",
                table: "UserHealthConditions",
                column: "HealthConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHealthConditions_NormalizedCustomHealthConditionName_trgm",
                table: "UserHealthConditions",
                column: "NormalizedCustomHealthConditionName",
                filter: "\"Source\" = 'Custom'")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_UserHealthConditions_UserId_HealthConditionId_Database",
                table: "UserHealthConditions",
                columns: new[] { "UserId", "HealthConditionId" },
                unique: true,
                filter: "\"Source\" = 'Database'");

            migrationBuilder.CreateIndex(
                name: "IX_UserHealthConditions_UserId_NormalizedCustomHealthConditionName_Custom",
                table: "UserHealthConditions",
                columns: new[] { "UserId", "NormalizedCustomHealthConditionName" },
                unique: true,
                filter: "\"Source\" = 'Custom'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_reproductive_health_profiles");

            migrationBuilder.DropTable(
                name: "healthcare_provider_profiles");

            migrationBuilder.DropTable(
                name: "pending_email_changes");

            migrationBuilder.DropTable(
                name: "UserHealthConditions");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "HealthConditions");
        }
    }
}
