using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstituteSettingsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstituteSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstituteId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AttendanceThresholdPercentage = table.Column<int>(type: "integer", nullable: false),
                    PassMarksPercentage = table.Column<int>(type: "integer", nullable: false),
                    MaxGraceMarks = table.Column<int>(type: "integer", nullable: false),
                    AutoLockAttendanceAfterDays = table.Column<bool>(type: "boolean", nullable: false),
                    AttendanceLockDays = table.Column<int>(type: "integer", nullable: false),
                    AutoLockPracticalAfterDays = table.Column<bool>(type: "boolean", nullable: false),
                    PracticalLockDays = table.Column<int>(type: "integer", nullable: false),
                    AuditLogRetentionDays = table.Column<int>(type: "integer", nullable: false),
                    EnableNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AcademicSessionFormat = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MaxStudentsPerBatch = table.Column<int>(type: "integer", nullable: false),
                    LogoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Website = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PrincipalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AffiliationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RecognitionNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstituteSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstituteSettings_Institutes_InstituteId",
                        column: x => x.InstituteId,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstituteSettings_InstituteId",
                table: "InstituteSettings",
                column: "InstituteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstituteSettings");
        }
    }
}
