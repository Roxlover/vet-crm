using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCrm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgerNoteToLedgerEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntries_Users_CreatedByUserId",
                table: "LedgerEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_LedgerEntries_Visits_VisitId",
                table: "LedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_CreatedByUserId",
                table: "LedgerEntries");

            migrationBuilder.DropIndex(
                name: "IX_LedgerEntries_VisitId",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "VisitId",
                table: "LedgerEntries");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "LedgerEntries",
                newName: "Note");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "LedgerEntries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "LedgerEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "LedgerEntries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Note",
                table: "LedgerEntries",
                newName: "Description");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "LedgerEntries",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "LedgerEntries",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "LedgerEntries",
                type: "numeric(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "LedgerEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VisitId",
                table: "LedgerEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_CreatedByUserId",
                table: "LedgerEntries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_VisitId",
                table: "LedgerEntries",
                column: "VisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntries_Users_CreatedByUserId",
                table: "LedgerEntries",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerEntries_Visits_VisitId",
                table: "LedgerEntries",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
