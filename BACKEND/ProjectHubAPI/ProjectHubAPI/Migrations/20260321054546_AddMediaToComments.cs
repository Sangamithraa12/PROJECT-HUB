using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectHubAPI.Migrations
{

    public partial class AddMediaToComments : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);

            
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "Comments");
        }
    }
}
