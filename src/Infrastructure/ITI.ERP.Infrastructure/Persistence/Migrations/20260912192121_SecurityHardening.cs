using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SecurityHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Batches",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticalMarks_InstituteId",
                table: "YearlyPracticalMarks",
                column: "InstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticalMarks_InstituteId",
                table: "PracticalMarks",
                column: "InstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PracticalMarks_Institutes_InstituteId",
                table: "PracticalMarks",
                column: "InstituteId",
                principalTable: "Institutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YearlyPracticalMarks_Institutes_InstituteId",
                table: "YearlyPracticalMarks",
                column: "InstituteId",
                principalTable: "Institutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PracticalMarks_Institutes_InstituteId",
                table: "PracticalMarks");

            migrationBuilder.DropForeignKey(
                name: "FK_YearlyPracticalMarks_Institutes_InstituteId",
                table: "YearlyPracticalMarks");

            migrationBuilder.DropIndex(
                name: "IX_YearlyPracticalMarks_InstituteId",
                table: "YearlyPracticalMarks");

            migrationBuilder.DropIndex(
                name: "IX_PracticalMarks_InstituteId",
                table: "PracticalMarks");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Batches");
        }
    }
}
