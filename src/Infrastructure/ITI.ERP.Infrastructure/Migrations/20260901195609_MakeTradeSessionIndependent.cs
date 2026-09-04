using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeTradeSessionIndependent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trades_InstituteId_AcademicSessionId_Code",
                table: "Trades");

            migrationBuilder.AlterColumn<Guid>(
                name: "AcademicSessionId",
                table: "Trades",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_InstituteId_Code",
                table: "Trades",
                columns: new[] { "InstituteId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trades_InstituteId_Code",
                table: "Trades");

            migrationBuilder.AlterColumn<Guid>(
                name: "AcademicSessionId",
                table: "Trades",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trades_InstituteId_AcademicSessionId_Code",
                table: "Trades",
                columns: new[] { "InstituteId", "AcademicSessionId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }
    }
}
