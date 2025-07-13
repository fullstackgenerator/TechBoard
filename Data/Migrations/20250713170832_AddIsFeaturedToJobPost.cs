using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechBoard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFeaturedToJobPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "JobPosts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "JobPosts");
        }
    }
}
