using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCrm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Visits",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusUpdatedAt",
                table: "Visits",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "StatusUpdatedAt",
                table: "Visits");
        }
    }
}
