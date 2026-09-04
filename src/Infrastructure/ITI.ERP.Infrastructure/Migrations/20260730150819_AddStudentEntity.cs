using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    BloodGroup = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FatherName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MotherName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuardianPhone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    GuardianRelation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AcademicSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RollNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AdmissionNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AnnualIncome = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    CasteCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsPhysicallyHandicapped = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousSchool = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PreviousQualification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PreviousPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StatusChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusChangedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AadharNumber = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    PhotoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    EmergencyContactRelation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InstituteId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DraftStatus = table.Column<int>(type: "integer", nullable: false),
                    DraftSavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DraftSavedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UnlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UnlockedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UnlockReason = table.Column<string>(type: "text", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalizedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_AcademicSessions_AcademicSessionId",
                        column: x => x.AcademicSessionId,
                        principalTable: "AcademicSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Students_Institutes_InstituteId",
                        column: x => x.InstituteId,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Students_Trades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "Trades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_AcademicSessionId",
                table: "Students",
                column: "AcademicSessionId");

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
                name: "IX_Students_TradeId",
                table: "Students",
                column: "TradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
