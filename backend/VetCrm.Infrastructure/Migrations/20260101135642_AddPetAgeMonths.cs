using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCrm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPetAgeMonths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_UserId_VisitId_IsIncome",
                table: "LedgerEntries");

            migrationBuilder.AddColumn<int>(
                name: "AgeMonths",
                table: "Pets",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_UserId",
                table: "LedgerEntries",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_UserId",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "AgeMonths",
                table: "Pets");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_UserId_VisitId_IsIncome",
                table: "LedgerEntries",
                columns: new[] { "UserId", "VisitId", "IsIncome" },
                unique: true,
                filter: "\"VisitId\" IS NOT NULL");
        }
    }
}
