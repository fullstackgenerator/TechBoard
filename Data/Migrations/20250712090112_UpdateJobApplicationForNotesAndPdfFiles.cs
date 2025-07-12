using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechBoard.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJobApplicationForNotesAndPdfFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverLetter",
                table: "JobApplications");

            migrationBuilder.RenameColumn(
                name: "CVFilePath",
                table: "JobApplications",
                newName: "CvFilePath");

            migrationBuilder.RenameColumn(
                name: "CVFileName",
                table: "JobApplications",
                newName: "CvFileName");

            migrationBuilder.AddColumn<string>(
                name: "ApplicantNotes",
                table: "JobApplications",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicantNotes",
                table: "JobApplications");

            migrationBuilder.RenameColumn(
                name: "CvFilePath",
                table: "JobApplications",
                newName: "CVFilePath");

            migrationBuilder.RenameColumn(
                name: "CvFileName",
                table: "JobApplications",
                newName: "CVFileName");

            migrationBuilder.AddColumn<string>(
                name: "CoverLetter",
                table: "JobApplications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
