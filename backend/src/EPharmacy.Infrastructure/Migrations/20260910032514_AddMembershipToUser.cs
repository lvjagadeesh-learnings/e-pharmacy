using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMember",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MembershipJoinedAtUtc",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMember",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MembershipJoinedAtUtc",
                table: "Users");
        }
    }
}
