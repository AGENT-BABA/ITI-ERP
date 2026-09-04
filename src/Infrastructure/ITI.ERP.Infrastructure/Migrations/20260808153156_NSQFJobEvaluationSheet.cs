using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NSQFJobEvaluationSheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dev data cleanup: delete existing PracticalMarks (old single-mark format)
            migrationBuilder.Sql("DELETE FROM \"PracticalMarks\";");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Mon~",
                table: "MonthlyPracticals");

            migrationBuilder.DropColumn(
                name: "MarksObtained",
                table: "PracticalMarks");

            migrationBuilder.DropColumn(
                name: "PassMarks",
                table: "MonthlyPracticals");

            migrationBuilder.DropColumn(
                name: "TotalMarks",
                table: "MonthlyPracticals");

            migrationBuilder.AddColumn<int>(
                name: "ApplicationKnowledge",
                table: "PracticalMarks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AttendancePunctuality",
                table: "PracticalMarks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowInstructions",
                table: "PracticalMarks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QualityWorkmanship",
                table: "PracticalMarks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SafetyConsciousness",
                table: "PracticalMarks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SignedByTrainee",
                table: "PracticalMarks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SkillsToolsEquipment",
                table: "PracticalMarks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SpeedDoingWork",
                table: "PracticalMarks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Viva",
                table: "PracticalMarks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkplaceHygiene",
                table: "PracticalMarks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AssessorName",
                table: "MonthlyPracticals",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "MonthlyPracticals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LearningOutcome",
                table: "MonthlyPracticals",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfessionalSkillName",
                table: "MonthlyPracticals",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "MonthlyPracticals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Mon~",
                table: "MonthlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "Month", "Year", "ProfessionalSkillName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Mon~",
                table: "MonthlyPracticals");

            migrationBuilder.DropColumn(
                name: "ApplicationKnowledge",
                table: "PracticalMarks");

            migrationBuilder.DropColumn(
                name: "AttendancePunctuality",
                table: "PracticalMarks");

            migrationBuilder.DropColumn(
                name: "FollowInstructions",
                table: "PracticalMarks");

            migrationBuilder.DropColumn(
                name: "QualityWorkmanship",
                table: "PracticalMarks");

            migrationBuilder.DropColumn(
                name: "SafetyConsciousness",
                table: "PracticalMarks");

            migrationBuilder.DropColumn(
                name: "SignedByTrainee",
                table: "PracticalMarks");

            migrationBuilder.DropColumn(
                name: "SkillsToolsEquipment",
                table: "PracticalMarks");

            migrationBuilder.DropColumn(
                name: "SpeedDoingWork",
                table: "PracticalMarks");

            migrationBuilder.DropColumn(
                name: "Viva",
                table: "PracticalMarks");

            migrationBuilder.DropColumn(
                name: "WorkplaceHygiene",
                table: "PracticalMarks");

            migrationBuilder.DropColumn(
                name: "AssessorName",
                table: "MonthlyPracticals");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "MonthlyPracticals");

            migrationBuilder.DropColumn(
                name: "LearningOutcome",
                table: "MonthlyPracticals");

            migrationBuilder.DropColumn(
                name: "ProfessionalSkillName",
                table: "MonthlyPracticals");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "MonthlyPracticals");

            migrationBuilder.AddColumn<decimal>(
                name: "MarksObtained",
                table: "PracticalMarks",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PassMarks",
                table: "MonthlyPracticals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalMarks",
                table: "MonthlyPracticals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Mon~",
                table: "MonthlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "Month", "Year" },
                unique: true);
        }
    }
}
