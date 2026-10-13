using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "study_clinics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    scenario_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_clinics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "study_clinic_members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    member_kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    login_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    protected_password = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    position_x = table.Column<double>(type: "double precision", nullable: false),
                    position_y = table.Column<double>(type: "double precision", nullable: false),
                    study_clinic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_clinic_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_study_clinic_members_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_study_clinic_members_study_clinics_study_clinic_id",
                        column: x => x.study_clinic_id,
                        principalTable: "study_clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_study_clinic_members_clinic_kind",
                table: "study_clinic_members",
                columns: new[] { "study_clinic_id", "member_kind" });

            migrationBuilder.CreateIndex(
                name: "ux_study_clinic_members_login_email",
                table: "study_clinic_members",
                column: "login_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_study_clinic_members_user_id",
                table: "study_clinic_members",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_study_clinics_created_at_utc",
                table: "study_clinics",
                column: "CreatedAtUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ux_study_clinics_normalized_name",
                table: "study_clinics",
                column: "normalized_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "study_clinic_members");

            migrationBuilder.DropTable(
                name: "study_clinics");
        }
    }
}
