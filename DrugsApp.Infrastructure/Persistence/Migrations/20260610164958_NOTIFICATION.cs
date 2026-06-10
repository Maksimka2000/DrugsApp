using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NOTIFICATION : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemAnnouncements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AudienceScopeCode = table.Column<string>(type: "text", nullable: false),
                    PrimaryTagCode = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SummaryText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BodyText = table.Column<string>(type: "text", nullable: false),
                    DeepLinkPath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ActiveFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActiveToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAnnouncements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemAnnouncementId = table.Column<Guid>(type: "uuid", nullable: true),
                    TypeCode = table.Column<string>(type: "text", nullable: false),
                    PrimaryTagCode = table.Column<string>(type: "text", nullable: false),
                    AudienceScopeCode = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SummaryText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BodyText = table.Column<string>(type: "text", nullable: true),
                    SourceEventKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RelatedAggregateType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RelatedAggregateId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeepLinkPath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RequiresAction = table.Column<bool>(type: "boolean", nullable: false),
                    ActionState = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNotifications_SystemAnnouncements_SystemAnnouncementId",
                        column: x => x.SystemAnnouncementId,
                        principalTable: "SystemAnnouncements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemAnnouncements_ActiveFromUtc_ActiveToUtc",
                table: "SystemAnnouncements",
                columns: new[] { "ActiveFromUtc", "ActiveToUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_SystemAnnouncementId",
                table: "UserNotifications",
                column: "SystemAnnouncementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId_AudienceScopeCode_CreatedAtUtc",
                table: "UserNotifications",
                columns: new[] { "UserId", "AudienceScopeCode", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId_IsRead_CreatedAtUtc_Id",
                table: "UserNotifications",
                columns: new[] { "UserId", "IsRead", "CreatedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId_SourceEventKey",
                table: "UserNotifications",
                columns: new[] { "UserId", "SourceEventKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropTable(
                name: "SystemAnnouncements");
        }
    }
}
