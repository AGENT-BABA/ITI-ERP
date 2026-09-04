using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalStabilization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trades_AcademicSessions_AcademicSessionId",
                table: "Trades");

            migrationBuilder.DropForeignKey(
                name: "FK_Trades_Institutes_InstituteId",
                table: "Trades");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_AcademicSessions_AcademicSessionId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Institutes_InstituteId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Trades_TradeId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions");

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicSessionId1",
                table: "Trades",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstituteId1",
                table: "Trades",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Module",
                table: "Permissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Permissions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Permissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "Permissions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId_AcademicSessionId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId", "AcademicSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trades_AcademicSessionId1",
                table: "Trades",
                column: "AcademicSessionId1");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_InstituteId1",
                table: "Trades",
                column: "InstituteId1");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_AcademicSessions_AcademicSessionId",
                table: "Trades",
                column: "AcademicSessionId",
                principalTable: "AcademicSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_AcademicSessions_AcademicSessionId1",
                table: "Trades",
                column: "AcademicSessionId1",
                principalTable: "AcademicSessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_Institutes_InstituteId",
                table: "Trades",
                column: "InstituteId",
                principalTable: "Institutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_Institutes_InstituteId1",
                table: "Trades",
                column: "InstituteId1",
                principalTable: "Institutes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_AcademicSessions_AcademicSessionId",
                table: "UserRoles",
                column: "AcademicSessionId",
                principalTable: "AcademicSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Institutes_InstituteId",
                table: "UserRoles",
                column: "InstituteId",
                principalTable: "Institutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Trades_TradeId",
                table: "UserRoles",
                column: "TradeId",
                principalTable: "Trades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trades_AcademicSessions_AcademicSessionId",
                table: "Trades");

            migrationBuilder.DropForeignKey(
                name: "FK_Trades_AcademicSessions_AcademicSessionId1",
                table: "Trades");

            migrationBuilder.DropForeignKey(
                name: "FK_Trades_Institutes_InstituteId",
                table: "Trades");

            migrationBuilder.DropForeignKey(
                name: "FK_Trades_Institutes_InstituteId1",
                table: "Trades");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_AcademicSessions_AcademicSessionId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Institutes_InstituteId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Trades_TradeId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId_RoleId_AcademicSessionId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Trades_AcademicSessionId1",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_InstituteId1",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleId_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Code",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "AcademicSessionId1",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "InstituteId1",
                table: "Trades");

            migrationBuilder.AlterColumn<string>(
                name: "Module",
                table: "Permissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Permissions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Permissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "Permissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_AcademicSessions_AcademicSessionId",
                table: "Trades",
                column: "AcademicSessionId",
                principalTable: "AcademicSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_Institutes_InstituteId",
                table: "Trades",
                column: "InstituteId",
                principalTable: "Institutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_AcademicSessions_AcademicSessionId",
                table: "UserRoles",
                column: "AcademicSessionId",
                principalTable: "AcademicSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Institutes_InstituteId",
                table: "UserRoles",
                column: "InstituteId",
                principalTable: "Institutes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Trades_TradeId",
                table: "UserRoles",
                column: "TradeId",
                principalTable: "Trades",
                principalColumn: "Id");
        }
    }
}
