using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddYearlyPracticalMonthlyEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnnualRemark",
                table: "YearlyPracticalMarks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualTotal",
                table: "YearlyPracticalMarks",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MonthlyManualEntries",
                table: "YearlyPracticalMarks",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnualRemark",
                table: "YearlyPracticalMarks");

            migrationBuilder.DropColumn(
                name: "AnnualTotal",
                table: "YearlyPracticalMarks");

            migrationBuilder.DropColumn(
                name: "MonthlyManualEntries",
                table: "YearlyPracticalMarks");
        }
    }
}
