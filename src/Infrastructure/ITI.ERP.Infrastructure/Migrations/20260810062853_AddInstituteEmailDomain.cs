using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstituteEmailDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_RoleId_AcademicSessionId",
                table: "UserRoles");

            migrationBuilder.AlterColumn<Guid>(
                name: "AcademicSessionId",
                table: "UserRoles",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "EmailDomain",
                table: "Institutes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId_InstituteId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId", "InstituteId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_RoleId_InstituteId",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "EmailDomain",
                table: "Institutes");

            migrationBuilder.AlterColumn<Guid>(
                name: "AcademicSessionId",
                table: "UserRoles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId_AcademicSessionId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId", "AcademicSessionId" },
                unique: true);
        }
    }
}
