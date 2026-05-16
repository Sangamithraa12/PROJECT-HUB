using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectHubAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskProofUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProofUrl",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProofUrl",
                table: "Tasks");
        }
    }
}
 
