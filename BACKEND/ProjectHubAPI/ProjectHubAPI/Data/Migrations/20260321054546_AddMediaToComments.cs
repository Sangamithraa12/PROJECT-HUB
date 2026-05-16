using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectHubAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaToComments : Migration
    {
        /// <inheritdoc />
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

            /* Commented out to avoid FK conflict with existing data */
            /*
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedTo",
                table: "Tasks",
                column: "AssignedTo");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_AssignedTo",
                table: "Tasks",
                column: "AssignedTo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_AssignedTo",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_AssignedTo",
                table: "Tasks");
            */

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "Comments");
        }
    }
}
 
