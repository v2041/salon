using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d4c60e8f-86b8-4634-8008-9c78df7e7347"),
                columns: new[] { "CreatedAt", "LastLoginAt", "Role" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("ff9152cd-b1b1-4218-9cdf-d80155e145d8"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d4c60e8f-86b8-4634-8008-9c78df7e7347"),
                columns: new[] { "CreatedAt", "LastLoginAt", "Role" },
                values: new object[] { new DateTime(2026, 9, 18, 18, 53, 16, 900, DateTimeKind.Utc).AddTicks(6594), new DateTime(2026, 9, 18, 18, 53, 16, 900, DateTimeKind.Utc).AddTicks(6594), 0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("ff9152cd-b1b1-4218-9cdf-d80155e145d8"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2026, 9, 18, 18, 53, 16, 901, DateTimeKind.Utc).AddTicks(1640), new DateTime(2026, 9, 18, 18, 53, 16, 901, DateTimeKind.Utc).AddTicks(1640) });
        }
    }
}
