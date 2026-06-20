using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceAssistant.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportDay",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportFormat",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportDay",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReportFormat",
                table: "Users");
        }
    }
}
