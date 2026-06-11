using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PATIENTandHEALTHCAREPROVIDER : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supervision_connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    healthcare_professional_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_level_code = table.Column<string>(type: "text", nullable: false),
                    connection_state = table.Column<string>(type: "text", nullable: false),
                    approved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    removed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supervision_connections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supervision_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    healthcare_professional_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_state = table.Column<string>(type: "text", nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    requested_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supervision_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "medical_change_proposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    supervision_connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    healthcare_professional_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proposal_state = table.Column<string>(type: "text", nullable: false),
                    access_level_snapshot = table.Column<string>(type: "text", nullable: false),
                    proposal_summary = table.Column<string>(type: "text", nullable: false),
                    submitted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_by_patient_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_change_proposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_medical_change_proposals_supervision_connections_supervisio~",
                        column: x => x.supervision_connection_id,
                        principalTable: "supervision_connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medical_change_proposal_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    proposal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_code = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    related_catalog_id = table.Column<Guid>(type: "uuid", nullable: true),
                    custom_value = table.Column<string>(type: "text", nullable: true),
                    display_name_snapshot = table.Column<string>(type: "text", nullable: false),
                    secondary_text_snapshot = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    apply_payload = table.Column<string>(type: "jsonb", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    MedicalChangeProposalId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_change_proposal_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_medical_change_proposal_items_medical_change_proposals_Medi~",
                        column: x => x.MedicalChangeProposalId,
                        principalTable: "medical_change_proposals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_medical_change_proposal_items_medical_change_proposals_prop~",
                        column: x => x.proposal_id,
                        principalTable: "medical_change_proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medical_change_proposal_item_intake_times",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    proposal_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    intake_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    MedicalChangeProposalItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_change_proposal_item_intake_times", x => x.Id);
                    table.ForeignKey(
                        name: "FK_medical_change_proposal_item_intake_times_medical_change_pr~",
                        column: x => x.MedicalChangeProposalItemId,
                        principalTable: "medical_change_proposal_items",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_medical_change_proposal_item_intake_times_medical_change_p~1",
                        column: x => x.proposal_item_id,
                        principalTable: "medical_change_proposal_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_medical_change_proposal_item_intake_times_MedicalChangeProp~",
                table: "medical_change_proposal_item_intake_times",
                column: "MedicalChangeProposalItemId");

            migrationBuilder.CreateIndex(
                name: "ux_medical_change_proposal_item_times_item_time",
                table: "medical_change_proposal_item_intake_times",
                columns: new[] { "proposal_item_id", "intake_time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_medical_change_proposal_items_MedicalChangeProposalId",
                table: "medical_change_proposal_items",
                column: "MedicalChangeProposalId");

            migrationBuilder.CreateIndex(
                name: "ux_medical_change_proposal_items_proposal_sort",
                table: "medical_change_proposal_items",
                columns: new[] { "proposal_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_medical_change_proposals_connection_state",
                table: "medical_change_proposals",
                columns: new[] { "supervision_connection_id", "proposal_state" });

            migrationBuilder.CreateIndex(
                name: "ix_medical_change_proposals_patient_state_submitted_desc_id",
                table: "medical_change_proposals",
                columns: new[] { "patient_user_id", "proposal_state", "submitted_at_utc", "Id" },
                descending: new[] { false, false, true, true });

            migrationBuilder.CreateIndex(
                name: "ix_medical_change_proposals_provider_state_submitted_desc_id",
                table: "medical_change_proposals",
                columns: new[] { "healthcare_professional_user_id", "proposal_state", "submitted_at_utc", "Id" },
                descending: new[] { false, false, true, true });

            migrationBuilder.CreateIndex(
                name: "ix_supervision_connections_patient_state_approved_desc_id",
                table: "supervision_connections",
                columns: new[] { "patient_user_id", "connection_state", "approved_at_utc", "Id" },
                descending: new[] { false, false, true, true });

            migrationBuilder.CreateIndex(
                name: "ix_supervision_connections_provider_state_approved_desc_id",
                table: "supervision_connections",
                columns: new[] { "healthcare_professional_user_id", "connection_state", "approved_at_utc", "Id" },
                descending: new[] { false, false, true, true });

            migrationBuilder.CreateIndex(
                name: "ux_supervision_connections_pair_active",
                table: "supervision_connections",
                columns: new[] { "patient_user_id", "healthcare_professional_user_id" },
                unique: true,
                filter: "connection_state = 'Active'");

            migrationBuilder.CreateIndex(
                name: "ix_supervision_requests_patient_requested_desc_id",
                table: "supervision_requests",
                columns: new[] { "patient_user_id", "requested_at_utc", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "ix_supervision_requests_provider_requested",
                table: "supervision_requests",
                columns: new[] { "healthcare_professional_user_id", "requested_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_supervision_requests_provider_requested_desc_id",
                table: "supervision_requests",
                columns: new[] { "healthcare_professional_user_id", "requested_at_utc", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "ix_supervision_requests_state_expires",
                table: "supervision_requests",
                columns: new[] { "request_state", "expires_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_supervision_requests_pair_pending",
                table: "supervision_requests",
                columns: new[] { "patient_user_id", "healthcare_professional_user_id" },
                unique: true,
                filter: "request_state = 'pending'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "medical_change_proposal_item_intake_times");

            migrationBuilder.DropTable(
                name: "supervision_requests");

            migrationBuilder.DropTable(
                name: "medical_change_proposal_items");

            migrationBuilder.DropTable(
                name: "medical_change_proposals");

            migrationBuilder.DropTable(
                name: "supervision_connections");
        }
    }
}
