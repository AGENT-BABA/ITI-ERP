using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RedesignBatchForMultiYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_AcademicSessions_AcademicSessionId",
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "YearLevel",
                table: "Batches");

            migrationBuilder.RenameColumn(
                name: "AcademicSessionId",
                table: "Batches",
                newName: "StartAcademicSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Batches_InstituteId_TradeId_AcademicSessionId_Code",
                table: "Batches",
                newName: "IX_Batches_InstituteId_TradeId_StartAcademicSessionId_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Batches_AcademicSessionId",
                table: "Batches",
                newName: "IX_Batches_StartAcademicSessionId");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Batches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql(@"
                UPDATE ""Batches"" b
                SET ""StartDate"" = s.""StartDate""
                FROM ""AcademicSessions"" s
                WHERE b.""StartAcademicSessionId"" = s.""Id""
                  AND b.""StartDate"" = '0001-01-01 00:00:00+00:00';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_StartDate",
                table: "Batches",
                column: "StartDate");

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_AcademicSessions_StartAcademicSessionId",
                table: "Batches",
                column: "StartAcademicSessionId",
                principalTable: "AcademicSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_AcademicSessions_StartAcademicSessionId",
                table: "Batches");

            migrationBuilder.DropIndex(
                name: "IX_Batches_StartDate",
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Batches");

            migrationBuilder.RenameColumn(
                name: "StartAcademicSessionId",
                table: "Batches",
                newName: "AcademicSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Batches_StartAcademicSessionId",
                table: "Batches",
                newName: "IX_Batches_AcademicSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_Batches_InstituteId_TradeId_StartAcademicSessionId_Code",
                table: "Batches",
                newName: "IX_Batches_InstituteId_TradeId_AcademicSessionId_Code");

            migrationBuilder.AddColumn<int>(
                name: "YearLevel",
                table: "Batches",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_AcademicSessions_AcademicSessionId",
                table: "Batches",
                column: "AcademicSessionId",
                principalTable: "AcademicSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
