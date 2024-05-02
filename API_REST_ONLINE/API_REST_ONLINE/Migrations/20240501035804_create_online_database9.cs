using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_REST_ONLINE.Migrations
{
    /// <inheritdoc />
    public partial class create_online_database9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "invitername",
                table: "pending_invites",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invitername",
                table: "pending_invites");
        }
    }
}
