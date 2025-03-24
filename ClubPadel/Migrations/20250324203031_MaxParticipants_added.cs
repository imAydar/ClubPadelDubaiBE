using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubPadel.Migrations
{
    /// <inheritdoc />
    public partial class MaxParticipants_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsOnHold",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxParticipants",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxParticipants",
                table: "Events");

            migrationBuilder.AlterColumn<bool>(
                name: "IsOnHold",
                table: "Events",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}
