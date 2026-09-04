using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedInstituteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "InstituteSettings");

            migrationBuilder.DropColumn(
                name: "AttendanceLockDays",
                table: "InstituteSettings");

            migrationBuilder.DropColumn(
                name: "AuditLogRetentionDays",
                table: "InstituteSettings");

            migrationBuilder.DropColumn(
                name: "AutoLockAttendanceAfterDays",
                table: "InstituteSettings");

            migrationBuilder.DropColumn(
                name: "AutoLockPracticalAfterDays",
                table: "InstituteSettings");

            migrationBuilder.DropColumn(
                name: "MaxGraceMarks",
                table: "InstituteSettings");

            migrationBuilder.DropColumn(
                name: "MaxStudentsPerBatch",
                table: "InstituteSettings");

            migrationBuilder.DropColumn(
                name: "PracticalLockDays",
                table: "InstituteSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcademicYear",
                table: "InstituteSettings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttendanceLockDays",
                table: "InstituteSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AuditLogRetentionDays",
                table: "InstituteSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AutoLockAttendanceAfterDays",
                table: "InstituteSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoLockPracticalAfterDays",
                table: "InstituteSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxGraceMarks",
                table: "InstituteSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxStudentsPerBatch",
                table: "InstituteSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PracticalLockDays",
                table: "InstituteSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
