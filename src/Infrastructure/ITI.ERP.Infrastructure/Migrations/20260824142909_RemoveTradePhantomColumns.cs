using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTradePhantomColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trades_AcademicSessions_AcademicSessionId1",
                table: "Trades");

            migrationBuilder.DropForeignKey(
                name: "FK_Trades_Institutes_InstituteId1",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_AcademicSessionId1",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_InstituteId1",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "AcademicSessionId1",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "InstituteId1",
                table: "Trades");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcademicSessionId1",
                table: "Trades",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstituteId1",
                table: "Trades",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trades_AcademicSessionId1",
                table: "Trades",
                column: "AcademicSessionId1");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_InstituteId1",
                table: "Trades",
                column: "InstituteId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_AcademicSessions_AcademicSessionId1",
                table: "Trades",
                column: "AcademicSessionId1",
                principalTable: "AcademicSessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_Institutes_InstituteId1",
                table: "Trades",
                column: "InstituteId1",
                principalTable: "Institutes",
                principalColumn: "Id");
        }
    }
}
