using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCrm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitCreatedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Visits",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CreatedByUserId",
                table: "Visits",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Users_CreatedByUserId",
                table: "Visits",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Users_CreatedByUserId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_CreatedByUserId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Visits");
        }
    }
}
