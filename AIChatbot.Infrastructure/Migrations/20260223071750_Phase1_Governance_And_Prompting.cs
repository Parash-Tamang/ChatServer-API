using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIChatbot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase1_Governance_And_Prompting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColumnsJson",
                table: "ResponseMetadata");

            migrationBuilder.RenameColumn(
                name: "TokenUsageJson",
                table: "ResponseMetadata",
                newName: "RoleUsed");

            migrationBuilder.RenameColumn(
                name: "RowsJson",
                table: "ResponseMetadata",
                newName: "LlmResponseJson");

            migrationBuilder.AddColumn<Guid>(
                name: "ConnectionStringId",
                table: "ResponseMetadata",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PromptSetId",
                table: "ResponseMetadata",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConnectionStrings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DatabaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AuthMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrustCertificate = table.Column<bool>(type: "bit", nullable: false),
                    ConnectionTimeout = table.Column<int>(type: "int", nullable: false),
                    DbIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionStrings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleColumnPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionStringId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ColumnName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleColumnPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleDbPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionStringId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleDbPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleTablePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionStringId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleTablePermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromptSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionStringId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromptSets_ConnectionStrings_ConnectionStringId",
                        column: x => x.ConnectionStringId,
                        principalTable: "ConnectionStrings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromptFunctions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromptSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FunctionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SystemPrompt = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptFunctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromptFunctions_PromptSets_PromptSetId",
                        column: x => x.PromptSetId,
                        principalTable: "PromptSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResponseMetadata_ConnectionStringId",
                table: "ResponseMetadata",
                column: "ConnectionStringId");

            migrationBuilder.CreateIndex(
                name: "IX_PromptFunctions_PromptSetId",
                table: "PromptFunctions",
                column: "PromptSetId");

            migrationBuilder.CreateIndex(
                name: "IX_PromptSets_ConnectionStringId",
                table: "PromptSets",
                column: "ConnectionStringId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResponseMetadata_ConnectionStrings_ConnectionStringId",
                table: "ResponseMetadata",
                column: "ConnectionStringId",
                principalTable: "ConnectionStrings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResponseMetadata_ConnectionStrings_ConnectionStringId",
                table: "ResponseMetadata");

            migrationBuilder.DropTable(
                name: "PromptFunctions");

            migrationBuilder.DropTable(
                name: "RoleColumnPermissions");

            migrationBuilder.DropTable(
                name: "RoleDbPermissions");

            migrationBuilder.DropTable(
                name: "RoleTablePermissions");

            migrationBuilder.DropTable(
                name: "PromptSets");

            migrationBuilder.DropTable(
                name: "ConnectionStrings");

            migrationBuilder.DropIndex(
                name: "IX_ResponseMetadata_ConnectionStringId",
                table: "ResponseMetadata");

            migrationBuilder.DropColumn(
                name: "ConnectionStringId",
                table: "ResponseMetadata");

            migrationBuilder.DropColumn(
                name: "PromptSetId",
                table: "ResponseMetadata");

            migrationBuilder.RenameColumn(
                name: "RoleUsed",
                table: "ResponseMetadata",
                newName: "TokenUsageJson");

            migrationBuilder.RenameColumn(
                name: "LlmResponseJson",
                table: "ResponseMetadata",
                newName: "RowsJson");

            migrationBuilder.AddColumn<string>(
                name: "ColumnsJson",
                table: "ResponseMetadata",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
