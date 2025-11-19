using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCrm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VisitPurpose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "Visits",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "Visits");
        }
    }
}
