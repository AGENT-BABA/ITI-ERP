using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITI.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTradePreviousDraftStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreviousDraftStatus",
                table: "Trades",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousDraftStatus",
                table: "Trades");
        }
    }
}
