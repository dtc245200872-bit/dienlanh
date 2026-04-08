using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dienlanh.Migrations
{
    /// <inheritdoc />
    public partial class addNameToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "RepairRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "RepairRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepairRequests_TechnicianId",
                table: "RepairRequests",
                column: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairRequests_Users_TechnicianId",
                table: "RepairRequests",
                column: "TechnicianId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairRequests_Users_TechnicianId",
                table: "RepairRequests");

            migrationBuilder.DropIndex(
                name: "IX_RepairRequests_TechnicianId",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "RepairRequests");
        }
    }
}
