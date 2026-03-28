using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class changeInMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DefaultMappingForColumnsAttributes",
                keyColumn: "Id",
                keyValue: new Guid("00dbb8b7-30d5-421f-96a2-2d9920e786ce"),
                column: "SourceColumnName",
                value: "Narzędzia edukacyjne dla osoby wykonującej zawód medyczny");

            migrationBuilder.UpdateData(
                table: "DefaultMappingForColumnsAttributes",
                keyColumn: "Id",
                keyValue: new Guid("35aecb0c-29ea-43ea-9a53-6608a7991ac4"),
                column: "SourceColumnName",
                value: "Droga podania - Gatunek - Tkanka - Okres karencji");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DefaultMappingForColumnsAttributes",
                keyColumn: "Id",
                keyValue: new Guid("00dbb8b7-30d5-421f-96a2-2d9920e786ce"),
                column: "SourceColumnName",
                value: "Materiały edukacyjne dla osoby wykonującej zawód medyczny");

            migrationBuilder.UpdateData(
                table: "DefaultMappingForColumnsAttributes",
                keyColumn: "Id",
                keyValue: new Guid("35aecb0c-29ea-43ea-9a53-6608a7991ac4"),
                column: "SourceColumnName",
                value: "Droga podania / Gatunek / Tkanka / Okres karencji");
        }
    }
}
