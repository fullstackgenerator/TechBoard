using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechBoard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCvAndResumeToJobApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CVFileName",
                table: "JobApplications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CVFilePath",
                table: "JobApplications",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVFileName",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CVFilePath",
                table: "JobApplications");
        }
    }
}
