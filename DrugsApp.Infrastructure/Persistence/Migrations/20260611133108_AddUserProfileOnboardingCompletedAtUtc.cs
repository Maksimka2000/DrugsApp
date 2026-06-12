using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrugsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileOnboardingCompletedAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardingCompletedAtUtc",
                table: "user_profiles",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnboardingCompletedAtUtc",
                table: "user_profiles");
        }
    }
}
