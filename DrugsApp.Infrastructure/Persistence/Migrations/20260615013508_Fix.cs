using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_medical_change_proposal_item_intake_times_medical_change_pr~",
                table: "medical_change_proposal_item_intake_times");

            migrationBuilder.DropForeignKey(
                name: "FK_medical_change_proposal_item_intake_times_medical_change_p~1",
                table: "medical_change_proposal_item_intake_times");

            migrationBuilder.DropForeignKey(
                name: "FK_medical_change_proposal_items_medical_change_proposals_Medi~",
                table: "medical_change_proposal_items");

            migrationBuilder.DropIndex(
                name: "IX_medical_change_proposal_items_MedicalChangeProposalId",
                table: "medical_change_proposal_items");

            migrationBuilder.DropIndex(
                name: "IX_medical_change_proposal_item_intake_times_MedicalChangeProp~",
                table: "medical_change_proposal_item_intake_times");

            migrationBuilder.DropColumn(
                name: "MedicalChangeProposalId",
                table: "medical_change_proposal_items");

            migrationBuilder.DropColumn(
                name: "MedicalChangeProposalItemId",
                table: "medical_change_proposal_item_intake_times");

            migrationBuilder.AddForeignKey(
                name: "FK_medical_change_proposal_item_intake_times_medical_change_pr~",
                table: "medical_change_proposal_item_intake_times",
                column: "proposal_item_id",
                principalTable: "medical_change_proposal_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_medical_change_proposal_item_intake_times_medical_change_pr~",
                table: "medical_change_proposal_item_intake_times");

            migrationBuilder.AddColumn<Guid>(
                name: "MedicalChangeProposalId",
                table: "medical_change_proposal_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MedicalChangeProposalItemId",
                table: "medical_change_proposal_item_intake_times",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_medical_change_proposal_items_MedicalChangeProposalId",
                table: "medical_change_proposal_items",
                column: "MedicalChangeProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_medical_change_proposal_item_intake_times_MedicalChangeProp~",
                table: "medical_change_proposal_item_intake_times",
                column: "MedicalChangeProposalItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_medical_change_proposal_item_intake_times_medical_change_pr~",
                table: "medical_change_proposal_item_intake_times",
                column: "MedicalChangeProposalItemId",
                principalTable: "medical_change_proposal_items",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_medical_change_proposal_item_intake_times_medical_change_p~1",
                table: "medical_change_proposal_item_intake_times",
                column: "proposal_item_id",
                principalTable: "medical_change_proposal_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_medical_change_proposal_items_medical_change_proposals_Medi~",
                table: "medical_change_proposal_items",
                column: "MedicalChangeProposalId",
                principalTable: "medical_change_proposals",
                principalColumn: "Id");
        }
    }
}
