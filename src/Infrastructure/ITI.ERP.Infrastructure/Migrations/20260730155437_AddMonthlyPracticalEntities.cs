using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyPracticalEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonthlyPracticals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstituteId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TotalMarks = table.Column<int>(type: "integer", nullable: false),
                    PassMarks = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyPracticals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyPracticals_AcademicSessions_AcademicSessionId",
                        column: x => x.AcademicSessionId,
                        principalTable: "AcademicSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyPracticals_Institutes_InstituteId",
                        column: x => x.InstituteId,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyPracticals_Trades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "Trades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonthlyPracticals_Users_LockedBy",
                        column: x => x.LockedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PracticalMarks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonthlyPracticalId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarksObtained = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MarkedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InstituteId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticalMarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticalMarks_MonthlyPracticals_MonthlyPracticalId",
                        column: x => x.MonthlyPracticalId,
                        principalTable: "MonthlyPracticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PracticalMarks_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PracticalMarks_Users_MarkedBy",
                        column: x => x.MarkedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPracticals_AcademicSessionId",
                table: "MonthlyPracticals",
                column: "AcademicSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Mon~",
                table: "MonthlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPracticals_LockedBy",
                table: "MonthlyPracticals",
                column: "LockedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPracticals_TradeId",
                table: "MonthlyPracticals",
                column: "TradeId");

            migrationBuilder.CreateIndex(
                name: "IX_PracticalMarks_MarkedBy",
                table: "PracticalMarks",
                column: "MarkedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PracticalMarks_MonthlyPracticalId_StudentId",
                table: "PracticalMarks",
                columns: new[] { "MonthlyPracticalId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PracticalMarks_StudentId",
                table: "PracticalMarks",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PracticalMarks");

            migrationBuilder.DropTable(
                name: "MonthlyPracticals");
        }
    }
}
