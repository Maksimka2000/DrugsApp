using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ADMINUSERSFEATURE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at_utc",
                table: "AspNetUsers",
                type: "timestamptz",
                nullable: false,
                defaultValueSql: "timezone('utc', now())");

            migrationBuilder.AddColumn<string>(
                name: "full_name",
                table: "AspNetUsers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_created_at_utc",
                table: "AspNetUsers",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_full_name",
                table: "AspNetUsers",
                column: "full_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_asp_net_users_created_at_utc",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "ix_asp_net_users_full_name",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "created_at_utc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "full_name",
                table: "AspNetUsers");
        }
    }
}
