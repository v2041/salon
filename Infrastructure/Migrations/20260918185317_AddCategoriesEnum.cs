using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriesEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("3f3bc361-39a1-4eb9-8993-4fa07e7d89cf"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("8cfe2e74-089c-42c1-99db-b95eb9f0a369"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FirstName", "LastLoginAt", "LastName", "Phone", "Role" },
                values: new object[,]
                {
                    { new Guid("d4c60e8f-86b8-4634-8008-9c78df7e7347"), new DateTime(2026, 9, 18, 18, 53, 16, 900, DateTimeKind.Utc).AddTicks(6594), "Admin", new DateTime(2026, 9, 18, 18, 53, 16, 900, DateTimeKind.Utc).AddTicks(6594), "Test", "+79991234567", 0 },
                    { new Guid("ff9152cd-b1b1-4218-9cdf-d80155e145d8"), new DateTime(2026, 9, 18, 18, 53, 16, 901, DateTimeKind.Utc).AddTicks(1640), "Client", new DateTime(2026, 9, 18, 18, 53, 16, 901, DateTimeKind.Utc).AddTicks(1640), "Test", "+79341234567", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d4c60e8f-86b8-4634-8008-9c78df7e7347"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("ff9152cd-b1b1-4218-9cdf-d80155e145d8"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FirstName", "LastLoginAt", "LastName", "Phone", "Role" },
                values: new object[,]
                {
                    { new Guid("3f3bc361-39a1-4eb9-8993-4fa07e7d89cf"), new DateTime(2026, 9, 18, 18, 47, 32, 355, DateTimeKind.Utc).AddTicks(9727), "Client", new DateTime(2026, 9, 18, 18, 47, 32, 355, DateTimeKind.Utc).AddTicks(9727), "Test", "+79341234567", 0 },
                    { new Guid("8cfe2e74-089c-42c1-99db-b95eb9f0a369"), new DateTime(2026, 9, 18, 18, 47, 32, 355, DateTimeKind.Utc).AddTicks(4644), "Admin", new DateTime(2026, 9, 18, 18, 47, 32, 355, DateTimeKind.Utc).AddTicks(4644), "Test", "+79991234567", 0 }
                });
        }
    }
}
