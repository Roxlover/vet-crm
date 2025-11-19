using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCrm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPetAgeYears : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgeYears",
                table: "Pets",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeYears",
                table: "Pets");
        }
    }
}
