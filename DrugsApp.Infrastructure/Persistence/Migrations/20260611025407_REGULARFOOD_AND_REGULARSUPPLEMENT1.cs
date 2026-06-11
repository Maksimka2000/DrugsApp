using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class REGULARFOOD_AND_REGULARSUPPLEMENT1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_supervision_requests_pair_pending",
                table: "supervision_requests");

            migrationBuilder.DropIndex(
                name: "ux_supervision_connections_pair_active",
                table: "supervision_connections");

            migrationBuilder.CreateIndex(
                name: "ux_supervision_requests_pair_pending",
                table: "supervision_requests",
                columns: new[] { "patient_user_id", "healthcare_professional_user_id" },
                unique: true,
                filter: "request_state = 'pending'");

            migrationBuilder.CreateIndex(
                name: "ux_supervision_connections_pair_active",
                table: "supervision_connections",
                columns: new[] { "patient_user_id", "healthcare_professional_user_id" },
                unique: true,
                filter: "connection_state = 'Active'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_supervision_requests_pair_pending",
                table: "supervision_requests");

            migrationBuilder.DropIndex(
                name: "ux_supervision_connections_pair_active",
                table: "supervision_connections");

            migrationBuilder.CreateIndex(
                name: "ux_supervision_requests_pair_pending",
                table: "supervision_requests",
                columns: new[] { "patient_user_id", "healthcare_professional_user_id" },
                unique: true,
                filter: "request_state = 'pending'");

            migrationBuilder.CreateIndex(
                name: "ux_supervision_connections_pair_active",
                table: "supervision_connections",
                columns: new[] { "patient_user_id", "healthcare_professional_user_id" },
                unique: true,
                filter: "connection_state = 'Active'");
        }
    }
}
