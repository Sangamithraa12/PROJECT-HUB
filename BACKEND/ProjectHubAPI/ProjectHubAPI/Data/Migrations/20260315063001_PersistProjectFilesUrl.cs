using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectHubAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class PersistProjectFilesUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilesUrl",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilesUrl",
                table: "Projects");
        }
    }
}
