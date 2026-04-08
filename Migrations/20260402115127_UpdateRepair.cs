using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dienlanh.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRepair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "RepairRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "RepairRequests",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "RepairRequests");
        }
    }
}
