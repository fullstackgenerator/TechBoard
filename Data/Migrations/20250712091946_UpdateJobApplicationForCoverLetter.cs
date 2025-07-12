using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechBoard.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJobApplicationForCoverLetter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CvFilePath",
                table: "JobApplications",
                newName: "CoverLetterFilePath");

            migrationBuilder.RenameColumn(
                name: "CvFileName",
                table: "JobApplications",
                newName: "CoverLetterFileName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CoverLetterFilePath",
                table: "JobApplications",
                newName: "CvFilePath");

            migrationBuilder.RenameColumn(
                name: "CoverLetterFileName",
                table: "JobApplications",
                newName: "CvFileName");
        }
    }
}
