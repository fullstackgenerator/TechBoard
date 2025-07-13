using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechBoard.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileSocial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Portfolio",
                table: "AspNetUsers",
                newName: "User_Website");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "User_Website",
                table: "AspNetUsers",
                newName: "Portfolio");
        }
    }
}
