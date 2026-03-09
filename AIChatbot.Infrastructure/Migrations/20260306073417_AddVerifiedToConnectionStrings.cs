using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIChatbot.Infrastructure.Migrations
{
    public partial class AddVerifiedToConnectionStrings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Verified",
                table: "ConnectionStrings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Verified",
                table: "ConnectionStrings");
        }
    }
}