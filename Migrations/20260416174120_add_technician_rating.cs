using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dienlanh.Migrations
{
    /// <inheritdoc />
    public partial class add_technician_rating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TechnicianRatedAt",
                table: "RepairRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechnicianRating",
                table: "RepairRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicianRatingComment",
                table: "RepairRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TechnicianRatedAt",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "TechnicianRating",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "TechnicianRatingComment",
                table: "RepairRequests");
        }
    }
}
