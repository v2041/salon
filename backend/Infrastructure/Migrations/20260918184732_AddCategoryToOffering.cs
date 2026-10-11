using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToOffering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("435ba206-442d-43a7-800e-a79bfbb0cdb4"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("fb76413c-5a89-40e0-b103-ecdf38cdc070"));

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Offerings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FirstName", "LastLoginAt", "LastName", "Phone", "Role" },
                values: new object[,]
                {
                    { new Guid("3f3bc361-39a1-4eb9-8993-4fa07e7d89cf"), new DateTime(2026, 9, 18, 18, 47, 32, 355, DateTimeKind.Utc).AddTicks(9727), "Client", new DateTime(2026, 9, 18, 18, 47, 32, 355, DateTimeKind.Utc).AddTicks(9727), "Test", "+79341234567", 0 },
                    { new Guid("8cfe2e74-089c-42c1-99db-b95eb9f0a369"), new DateTime(2026, 9, 18, 18, 47, 32, 355, DateTimeKind.Utc).AddTicks(4644), "Admin", new DateTime(2026, 9, 18, 18, 47, 32, 355, DateTimeKind.Utc).AddTicks(4644), "Test", "+79991234567", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("3f3bc361-39a1-4eb9-8993-4fa07e7d89cf"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("8cfe2e74-089c-42c1-99db-b95eb9f0a369"));

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Offerings");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FirstName", "LastLoginAt", "LastName", "Phone", "Role" },
                values: new object[,]
                {
                    { new Guid("435ba206-442d-43a7-800e-a79bfbb0cdb4"), new DateTime(2026, 9, 9, 8, 20, 43, 140, DateTimeKind.Utc).AddTicks(4662), "Admin", new DateTime(2026, 9, 9, 8, 20, 43, 140, DateTimeKind.Utc).AddTicks(4662), "Test", "+79991234567", 0 },
                    { new Guid("fb76413c-5a89-40e0-b103-ecdf38cdc070"), new DateTime(2026, 9, 9, 8, 20, 43, 141, DateTimeKind.Utc).AddTicks(344), "Client", new DateTime(2026, 9, 9, 8, 20, 43, 141, DateTimeKind.Utc).AddTicks(344), "Test", "+79341234567", 0 }
                });
        }
    }
}
