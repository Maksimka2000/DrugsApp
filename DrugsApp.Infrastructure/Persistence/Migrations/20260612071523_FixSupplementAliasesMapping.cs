using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixSupplementAliasesMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<string>>(
                name: "Aliases",
                table: "Supplements",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(IReadOnlyCollection<string>),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<IReadOnlyCollection<string>>(
                name: "Aliases",
                table: "Supplements",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
