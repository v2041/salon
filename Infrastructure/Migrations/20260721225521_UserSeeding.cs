using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FirstName", "IsBot", "LastName", "MaxId", "Phone", "Role", "Username" },
                values: new object[,]
                {
                    { new Guid("b6e24b48-7da1-40e8-ad95-cbe84af58f32"), new DateTime(2026, 7, 21, 22, 55, 20, 573, DateTimeKind.Utc).AddTicks(9821), "Client", false, "Test", 12345L, "+79341234567", 0, "default" },
                    { new Guid("d5a15f81-c6fb-435e-a79e-006fcc79e4d2"), new DateTime(2026, 7, 21, 22, 55, 20, 573, DateTimeKind.Utc).AddTicks(4348), "Admin", false, "null", 1234L, "+79991234567", 0, "admin" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b6e24b48-7da1-40e8-ad95-cbe84af58f32"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d5a15f81-c6fb-435e-a79e-006fcc79e4d2"));
        }
    }
}
