using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddYearlyPracticalEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YearlyPracticals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstituteId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_YearlyPracticals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YearlyPracticals_AcademicSessions_AcademicSessionId",
                        column: x => x.AcademicSessionId,
                        principalTable: "AcademicSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YearlyPracticals_Institutes_InstituteId",
                        column: x => x.InstituteId,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YearlyPracticals_Trades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "Trades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YearlyPracticals_Users_LockedBy",
                        column: x => x.LockedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YearlyPracticalMarks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    YearlyPracticalId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_YearlyPracticalMarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YearlyPracticalMarks_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YearlyPracticalMarks_Users_MarkedBy",
                        column: x => x.MarkedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YearlyPracticalMarks_YearlyPracticals_YearlyPracticalId",
                        column: x => x.YearlyPracticalId,
                        principalTable: "YearlyPracticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticalMarks_MarkedBy",
                table: "YearlyPracticalMarks",
                column: "MarkedBy");

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticalMarks_StudentId",
                table: "YearlyPracticalMarks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticalMarks_YearlyPracticalId_StudentId",
                table: "YearlyPracticalMarks",
                columns: new[] { "YearlyPracticalId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticals_AcademicSessionId",
                table: "YearlyPracticals",
                column: "AcademicSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticals_InstituteId_AcademicSessionId_TradeId_Year",
                table: "YearlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticals_LockedBy",
                table: "YearlyPracticals",
                column: "LockedBy");

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticals_TradeId",
                table: "YearlyPracticals",
                column: "TradeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YearlyPracticalMarks");

            migrationBuilder.DropTable(
                name: "YearlyPracticals");
        }
    }
}
