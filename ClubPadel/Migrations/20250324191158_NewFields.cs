using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubPadel.Migrations
{
    /// <inheritdoc />
    public partial class NewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnWaitList",
                table: "Events");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "Events",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnHold",
                table: "Events",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Events",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SendAt",
                table: "Events",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsOnHold",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SendAt",
                table: "Events");

            migrationBuilder.AddColumn<bool>(
                name: "IsOnWaitList",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
