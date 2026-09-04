using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUniqueIndexFiltersOnSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_YearlyPracticals_InstituteId_AcademicSessionId_TradeId_Batc~",
                table: "YearlyPracticals");

            migrationBuilder.DropIndex(
                name: "IX_Users_InstituteId_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Trades_InstituteId_AcademicSessionId_Code",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Students_InstituteId_AcademicSessionId_AdmissionNumber",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_InstituteId_AcademicSessionId_RollNumber",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Bat~",
                table: "MonthlyPracticals");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_InstituteId_AcademicSessionId_StudentId_D~",
                table: "AttendanceRecords");

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticals_InstituteId_AcademicSessionId_TradeId_Batc~",
                table: "YearlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "BatchId", "Year" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Users_InstituteId_Username",
                table: "Users",
                columns: new[] { "InstituteId", "Username" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_InstituteId_AcademicSessionId_Code",
                table: "Trades",
                columns: new[] { "InstituteId", "AcademicSessionId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Students_InstituteId_AcademicSessionId_AdmissionNumber",
                table: "Students",
                columns: new[] { "InstituteId", "AcademicSessionId", "AdmissionNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Students_InstituteId_AcademicSessionId_RollNumber",
                table: "Students",
                columns: new[] { "InstituteId", "AcademicSessionId", "RollNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Bat~",
                table: "MonthlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "BatchId", "Month", "Year", "ProfessionalSkillName" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ProfessionalSkillName\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_InstituteId_AcademicSessionId_StudentId_D~",
                table: "AttendanceRecords",
                columns: new[] { "InstituteId", "AcademicSessionId", "StudentId", "Date" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_YearlyPracticals_InstituteId_AcademicSessionId_TradeId_Batc~",
                table: "YearlyPracticals");

            migrationBuilder.DropIndex(
                name: "IX_Users_InstituteId_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Trades_InstituteId_AcademicSessionId_Code",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Students_InstituteId_AcademicSessionId_AdmissionNumber",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_InstituteId_AcademicSessionId_RollNumber",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Bat~",
                table: "MonthlyPracticals");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_InstituteId_AcademicSessionId_StudentId_D~",
                table: "AttendanceRecords");

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticals_InstituteId_AcademicSessionId_TradeId_Batc~",
                table: "YearlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "BatchId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_InstituteId_Username",
                table: "Users",
                columns: new[] { "InstituteId", "Username" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trades_InstituteId_AcademicSessionId_Code",
                table: "Trades",
                columns: new[] { "InstituteId", "AcademicSessionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_InstituteId_AcademicSessionId_AdmissionNumber",
                table: "Students",
                columns: new[] { "InstituteId", "AcademicSessionId", "AdmissionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_InstituteId_AcademicSessionId_RollNumber",
                table: "Students",
                columns: new[] { "InstituteId", "AcademicSessionId", "RollNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Bat~",
                table: "MonthlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "BatchId", "Month", "Year", "ProfessionalSkillName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_InstituteId_AcademicSessionId_StudentId_D~",
                table: "AttendanceRecords",
                columns: new[] { "InstituteId", "AcademicSessionId", "StudentId", "Date" },
                unique: true);
        }
    }
}
