using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleToChatSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "UserSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "UserSessions");
        }
    }
}
