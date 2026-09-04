using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixHolidayUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Holidays_InstituteId_AcademicSessionId_Date",
                table: "Holidays");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_InstituteId_AcademicSessionId_Date",
                table: "Holidays",
                columns: new[] { "InstituteId", "AcademicSessionId", "Date" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Holidays_InstituteId_AcademicSessionId_Date",
                table: "Holidays");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_InstituteId_AcademicSessionId_Date",
                table: "Holidays",
                columns: new[] { "InstituteId", "AcademicSessionId", "Date" },
                unique: true);
        }
    }
}
