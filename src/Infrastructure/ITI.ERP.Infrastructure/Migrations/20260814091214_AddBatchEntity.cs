using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =============================================
            // STEP 1: Create Batches table FIRST (no FK dependencies)
            // =============================================
            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstituteId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batches_AcademicSessions_AcademicSessionId",
                        column: x => x.AcademicSessionId,
                        principalTable: "AcademicSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Batches_Institutes_InstituteId",
                        column: x => x.InstituteId,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Batches_Trades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "Trades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Batches indexes
            migrationBuilder.CreateIndex(
                name: "IX_Batches_AcademicSessionId",
                table: "Batches",
                column: "AcademicSessionId");
            migrationBuilder.CreateIndex(
                name: "IX_Batches_InstituteId",
                table: "Batches",
                column: "InstituteId");
            migrationBuilder.CreateIndex(
                name: "IX_Batches_InstituteId_TradeId_AcademicSessionId_Code",
                table: "Batches",
                columns: new[] { "InstituteId", "TradeId", "AcademicSessionId", "Code" },
                unique: true,
                filter: "\"Code\" IS NOT NULL AND \"IsDeleted\" = false");
            migrationBuilder.CreateIndex(
                name: "IX_Batches_TradeId",
                table: "Batches",
                column: "TradeId");

            // =============================================
            // STEP 2: Add nullable BatchId columns to all tables
            // =============================================
            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "Students",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "UserRoles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "AttendanceRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "MonthlyPracticals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "YearlyPracticals",
                type: "uuid",
                nullable: true);

            // =============================================
            // STEP 3: Create Default batches per unique Trade+AcademicSession
            // =============================================
            migrationBuilder.Sql(@"
                INSERT INTO ""Batches"" (""Id"", ""InstituteId"", ""TradeId"", ""AcademicSessionId"", ""Name"", ""Code"", ""IsActive"", ""IsDeleted"", ""CreatedAt"")
                SELECT
                    gen_random_uuid(),
                    t.""InstituteId"",
                    t.""Id"",
                    t.""AcademicSessionId"",
                    'Default',
                    'DEFAULT',
                    TRUE,
                    FALSE,
                    NOW()
                FROM ""Trades"" t
                WHERE NOT t.""IsDeleted""
                AND NOT EXISTS (
                    SELECT 1 FROM ""Batches"" b
                    WHERE b.""TradeId"" = t.""Id""
                    AND b.""AcademicSessionId"" = t.""AcademicSessionId""
                    AND b.""IsDeleted"" = FALSE
                );
            ");

            // =============================================
            // STEP 4: Backfill Students.BatchId from Default batch
            // =============================================
            migrationBuilder.Sql(@"
                UPDATE ""Students"" s
                SET ""BatchId"" = b.""Id""
                FROM ""Batches"" b
                WHERE b.""TradeId"" = s.""TradeId""
                AND b.""AcademicSessionId"" = s.""AcademicSessionId""
                AND b.""Code"" = 'DEFAULT' AND b.""IsDeleted"" = FALSE
                AND s.""BatchId"" IS NULL;
            ");

            // =============================================
            // STEP 5: Backfill UserRoles.BatchId for TradeHead assignments
            // =============================================
            migrationBuilder.Sql(@"
                UPDATE ""UserRoles"" ur
                SET ""BatchId"" = sub.""BatchId""
                FROM (
                    SELECT ur2.""Id"" AS ""UserRoleId"", b.""Id"" AS ""BatchId""
                    FROM ""UserRoles"" ur2
                    INNER JOIN ""Roles"" r ON r.""Id"" = ur2.""RoleId""
                    INNER JOIN ""Batches"" b
                        ON b.""TradeId"" = ur2.""TradeId""
                        AND b.""AcademicSessionId"" = ur2.""AcademicSessionId""
                        AND b.""InstituteId"" = ur2.""InstituteId""
                        AND b.""Code"" = 'DEFAULT' AND b.""IsDeleted"" = FALSE
                    WHERE r.""Name"" = 'TradeHead'
                    AND ur2.""BatchId"" IS NULL
                ) sub
                WHERE ur.""Id"" = sub.""UserRoleId"";
            ");

            // =============================================
            // STEP 6: Backfill MonthlyPracticals.BatchId from Default batch
            // =============================================
            migrationBuilder.Sql(@"
                UPDATE ""MonthlyPracticals"" mp
                SET ""BatchId"" = b.""Id""
                FROM ""Batches"" b
                WHERE b.""TradeId"" = mp.""TradeId""
                AND b.""AcademicSessionId"" = mp.""AcademicSessionId""
                AND b.""Code"" = 'DEFAULT' AND b.""IsDeleted"" = FALSE
                AND mp.""BatchId"" IS NULL;
            ");

            // =============================================
            // STEP 7: Backfill YearlyPracticals.BatchId from Default batch
            // =============================================
            migrationBuilder.Sql(@"
                UPDATE ""YearlyPracticals"" yp
                SET ""BatchId"" = b.""Id""
                FROM ""Batches"" b
                WHERE b.""TradeId"" = yp.""TradeId""
                AND b.""AcademicSessionId"" = yp.""AcademicSessionId""
                AND b.""Code"" = 'DEFAULT' AND b.""IsDeleted"" = FALSE
                AND yp.""BatchId"" IS NULL;
            ");

            // =============================================
            // STEP 8: Backfill AttendanceRecords.BatchId via Student join
            // =============================================
            migrationBuilder.Sql(@"
                UPDATE ""AttendanceRecords"" ar
                SET ""BatchId"" = s.""BatchId""
                FROM ""Students"" s
                WHERE s.""Id"" = ar.""StudentId""
                AND ar.""BatchId"" IS NULL;
            ");

            // =============================================
            // STEP 9: SAFETY CHECK — Fail if any NULL BatchIds remain
            //          on tables that require NOT NULL
            // =============================================
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    v_null_count BIGINT;
                    v_table_name TEXT;
                BEGIN
                    -- Check MonthlyPracticals
                    SELECT COUNT(*) INTO v_null_count FROM ""MonthlyPracticals"" WHERE ""BatchId"" IS NULL;
                    IF v_null_count > 0 THEN
                        RAISE EXCEPTION 'Migration safety check failed: % NULL BatchId values remain in MonthlyPracticals. All practicals must have a batch assignment.', v_null_count;
                    END IF;

                    -- Check YearlyPracticals
                    SELECT COUNT(*) INTO v_null_count FROM ""YearlyPracticals"" WHERE ""BatchId"" IS NULL;
                    IF v_null_count > 0 THEN
                        RAISE EXCEPTION 'Migration safety check failed: % NULL BatchId values remain in YearlyPracticals. All practicals must have a batch assignment.', v_null_count;
                    END IF;

                    -- Check AttendanceRecords
                    SELECT COUNT(*) INTO v_null_count FROM ""AttendanceRecords"" WHERE ""BatchId"" IS NULL;
                    IF v_null_count > 0 THEN
                        RAISE EXCEPTION 'Migration safety check failed: % NULL BatchId values remain in AttendanceRecords. All attendance records must have a batch assignment.', v_null_count;
                    END IF;

                    RAISE NOTICE 'Migration safety check passed: zero NULL BatchIds in MonthlyPracticals, YearlyPracticals, AttendanceRecords.';
                END $$;
            ");

            // =============================================
            // STEP 10: Make BatchId NOT NULL on required tables
            // =============================================
            migrationBuilder.AlterColumn<Guid>(
                name: "BatchId",
                table: "MonthlyPracticals",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BatchId",
                table: "YearlyPracticals",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BatchId",
                table: "AttendanceRecords",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // =============================================
            // STEP 11: Drop old unique indexes, create new ones with BatchId
            // =============================================
            migrationBuilder.DropIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Mon~",
                table: "MonthlyPracticals");

            migrationBuilder.DropIndex(
                name: "IX_YearlyPracticals_InstituteId_AcademicSessionId_TradeId_Year",
                table: "YearlyPracticals");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Bat~",
                table: "MonthlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "BatchId", "Month", "Year", "ProfessionalSkillName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticals_InstituteId_AcademicSessionId_TradeId_Batc~",
                table: "YearlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "BatchId", "Year" },
                unique: true);

            // =============================================
            // STEP 12: Add FK constraints and remaining indexes
            // =============================================
            migrationBuilder.CreateIndex(
                name: "IX_Students_BatchId",
                table: "Students",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_BatchId",
                table: "UserRoles",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPracticals_BatchId",
                table: "MonthlyPracticals",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticals_BatchId",
                table: "YearlyPracticals",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_BatchId",
                table: "AttendanceRecords",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Batches_BatchId",
                table: "Students",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Batches_BatchId",
                table: "UserRoles",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyPracticals_Batches_BatchId",
                table: "MonthlyPracticals",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YearlyPracticals_Batches_BatchId",
                table: "YearlyPracticals",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Batches_BatchId",
                table: "AttendanceRecords",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Batches_BatchId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Batches_BatchId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyPracticals_Batches_BatchId",
                table: "MonthlyPracticals");

            migrationBuilder.DropForeignKey(
                name: "FK_YearlyPracticals_Batches_BatchId",
                table: "YearlyPracticals");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Batches_BatchId",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_Students_BatchId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_BatchId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyPracticals_BatchId",
                table: "MonthlyPracticals");

            migrationBuilder.DropIndex(
                name: "IX_YearlyPracticals_BatchId",
                table: "YearlyPracticals");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_BatchId",
                table: "AttendanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Bat~",
                table: "MonthlyPracticals");

            migrationBuilder.DropIndex(
                name: "IX_YearlyPracticals_InstituteId_AcademicSessionId_TradeId_Batc~",
                table: "YearlyPracticals");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "MonthlyPracticals");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "YearlyPracticals");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyPracticals_InstituteId_AcademicSessionId_TradeId_Mon~",
                table: "MonthlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "Month", "Year", "ProfessionalSkillName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YearlyPracticals_InstituteId_AcademicSessionId_TradeId_Year",
                table: "YearlyPracticals",
                columns: new[] { "InstituteId", "AcademicSessionId", "TradeId", "Year" },
                unique: true);
        }
    }
}
