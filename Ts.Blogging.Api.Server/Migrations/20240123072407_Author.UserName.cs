using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ts.Blogging.Api.Server.Migrations
{
    /// <inheritdoc />
    public partial class AuthorUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Authors",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Authors");
        }
    }
}
