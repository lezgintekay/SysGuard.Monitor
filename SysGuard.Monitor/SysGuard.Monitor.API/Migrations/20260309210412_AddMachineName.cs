using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SysGuard.Monitor.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MachineName",
                table: "Metrics",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MachineName",
                table: "Metrics");
        }
    }
}
