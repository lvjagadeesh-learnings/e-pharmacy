using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPharmacy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderReceivedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReceivedAtUtc",
                table: "Orders",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivedAtUtc",
                table: "Orders");
        }
    }
}
