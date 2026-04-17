using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCrm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgerUserVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "LedgerEntries",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "LedgerEntries",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "LedgerEntries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VisitId",
                table: "LedgerEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_UserId_VisitId_IsIncome",
                table: "LedgerEntries",
                columns: new[] { "UserId", "VisitId", "IsIncome" },
                unique: true,
                filter: "\"VisitId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_VisitId",
                table: "LedgerEntries",
                column: "VisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntries_Users_UserId",
                table: "LedgerEntries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntries_Visits_VisitId",
                table: "LedgerEntries",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntries_Users_UserId",
                table: "LedgerEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntries_Visits_VisitId",
                table: "LedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_UserId_VisitId_IsIncome",
                table: "LedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_VisitId",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "VisitId",
                table: "LedgerEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "LedgerEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "LedgerEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
