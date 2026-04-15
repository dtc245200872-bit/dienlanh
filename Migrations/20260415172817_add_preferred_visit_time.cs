using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dienlanh.Migrations
{
    /// <inheritdoc />
    public partial class add_preferred_visit_time : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PreferredVisitAt",
                table: "RepairRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredVisitAt",
                table: "RepairRequests");
        }
    }
}
