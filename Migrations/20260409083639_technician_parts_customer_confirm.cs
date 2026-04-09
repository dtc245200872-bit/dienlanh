using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dienlanh.Migrations
{
    /// <inheritdoc />
    public partial class technician_parts_customer_confirm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerReportNote",
                table: "RepairRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomerReported",
                table: "RepairRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalAmount",
                table: "RepairRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PartsConfirmedByCustomer",
                table: "RepairRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReplacedParts",
                table: "RepairRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReportedAt",
                table: "RepairRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerReportNote",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "CustomerReported",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "PartsConfirmedByCustomer",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "ReplacedParts",
                table: "RepairRequests");

            migrationBuilder.DropColumn(
                name: "ReportedAt",
                table: "RepairRequests");
        }
    }
}
