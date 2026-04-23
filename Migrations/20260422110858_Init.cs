using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dienlanh.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "Users",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TotalJobsCompleted",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkEndHour",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkStartHour",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalJobsCompleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WorkEndHour",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WorkStartHour",
                table: "Users");
        }
    }
}
