using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAcademicSessionUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AcademicSessions_InstituteId_SessionYear",
                table: "AcademicSessions");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicSessions_InstituteId_SessionYear",
                table: "AcademicSessions",
                columns: new[] { "InstituteId", "SessionYear" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AcademicSessions_InstituteId_SessionYear",
                table: "AcademicSessions");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicSessions_InstituteId_SessionYear",
                table: "AcademicSessions",
                columns: new[] { "InstituteId", "SessionYear" },
                unique: true);
        }
    }
}
