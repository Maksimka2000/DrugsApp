using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FIX10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientDrugAllergies_Drugs_DrugId",
                table: "ClientDrugAllergies");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugProblems_Drugs_DrugId",
                table: "DrugProblems");

            migrationBuilder.DropIndex(
                name: "UX_DrugProblems_UserId_DrugId",
                table: "DrugProblems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DrugProblems_SourceXor",
                table: "DrugProblems");

            migrationBuilder.DropIndex(
                name: "IX_ClientDrugAllergies_UserId_DrugId_Database",
                table: "ClientDrugAllergies");

            migrationBuilder.RenameColumn(
                name: "DrugId",
                table: "DrugProblems",
                newName: "DrugFormId");

            migrationBuilder.RenameIndex(
                name: "IX_DrugProblems_DrugId",
                table: "DrugProblems",
                newName: "IX_DrugProblems_DrugFormId");

            migrationBuilder.RenameColumn(
                name: "DrugId",
                table: "ClientDrugAllergies",
                newName: "DrugFormId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientDrugAllergies_DrugId",
                table: "ClientDrugAllergies",
                newName: "IX_ClientDrugAllergies_DrugFormId");

            migrationBuilder.AddColumn<string>(
                name: "CustomActiveSubstances",
                table: "DrugProblems",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomDosage",
                table: "DrugProblems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomFormulationName",
                table: "DrugProblems",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomDrugNameNormalized",
                table: "ClientDrugAllergies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomDrugName",
                table: "ClientDrugAllergies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomActiveSubstances",
                table: "ClientDrugAllergies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomDosage",
                table: "ClientDrugAllergies",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomFormulationName",
                table: "ClientDrugAllergies",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_DrugProblems_UserId_DrugFormId",
                table: "DrugProblems",
                columns: new[] { "UserId", "DrugFormId" },
                unique: true,
                filter: "\"DrugFormId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DrugProblems_SourceXor",
                table: "DrugProblems",
                sql: "(CASE WHEN \"DrugFormId\" IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN \"CustomDrugName\" IS NOT NULL THEN 1 ELSE 0 END) = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDrugAllergies_UserId_DrugFormId_Database",
                table: "ClientDrugAllergies",
                columns: new[] { "UserId", "DrugFormId" },
                unique: true,
                filter: "\"DrugFormId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientDrugAllergies_DrugForms_DrugFormId",
                table: "ClientDrugAllergies",
                column: "DrugFormId",
                principalTable: "DrugForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugProblems_DrugForms_DrugFormId",
                table: "DrugProblems",
                column: "DrugFormId",
                principalTable: "DrugForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientDrugAllergies_DrugForms_DrugFormId",
                table: "ClientDrugAllergies");

            migrationBuilder.DropForeignKey(
                name: "FK_DrugProblems_DrugForms_DrugFormId",
                table: "DrugProblems");

            migrationBuilder.DropIndex(
                name: "UX_DrugProblems_UserId_DrugFormId",
                table: "DrugProblems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DrugProblems_SourceXor",
                table: "DrugProblems");

            migrationBuilder.DropIndex(
                name: "IX_ClientDrugAllergies_UserId_DrugFormId_Database",
                table: "ClientDrugAllergies");

            migrationBuilder.DropColumn(
                name: "CustomActiveSubstances",
                table: "DrugProblems");

            migrationBuilder.DropColumn(
                name: "CustomDosage",
                table: "DrugProblems");

            migrationBuilder.DropColumn(
                name: "CustomFormulationName",
                table: "DrugProblems");

            migrationBuilder.DropColumn(
                name: "CustomActiveSubstances",
                table: "ClientDrugAllergies");

            migrationBuilder.DropColumn(
                name: "CustomDosage",
                table: "ClientDrugAllergies");

            migrationBuilder.DropColumn(
                name: "CustomFormulationName",
                table: "ClientDrugAllergies");

            migrationBuilder.RenameColumn(
                name: "DrugFormId",
                table: "DrugProblems",
                newName: "DrugId");

            migrationBuilder.RenameIndex(
                name: "IX_DrugProblems_DrugFormId",
                table: "DrugProblems",
                newName: "IX_DrugProblems_DrugId");

            migrationBuilder.RenameColumn(
                name: "DrugFormId",
                table: "ClientDrugAllergies",
                newName: "DrugId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientDrugAllergies_DrugFormId",
                table: "ClientDrugAllergies",
                newName: "IX_ClientDrugAllergies_DrugId");

            migrationBuilder.AlterColumn<string>(
                name: "CustomDrugNameNormalized",
                table: "ClientDrugAllergies",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomDrugName",
                table: "ClientDrugAllergies",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_DrugProblems_UserId_DrugId",
                table: "DrugProblems",
                columns: new[] { "UserId", "DrugId" },
                unique: true,
                filter: "\"DrugId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DrugProblems_SourceXor",
                table: "DrugProblems",
                sql: "(CASE WHEN \"DrugId\" IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN \"CustomDrugName\" IS NOT NULL THEN 1 ELSE 0 END) = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ClientDrugAllergies_UserId_DrugId_Database",
                table: "ClientDrugAllergies",
                columns: new[] { "UserId", "DrugId" },
                unique: true,
                filter: "\"DrugId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientDrugAllergies_Drugs_DrugId",
                table: "ClientDrugAllergies",
                column: "DrugId",
                principalTable: "Drugs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrugProblems_Drugs_DrugId",
                table: "DrugProblems",
                column: "DrugId",
                principalTable: "Drugs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
