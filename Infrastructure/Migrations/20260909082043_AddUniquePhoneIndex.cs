using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePhoneIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("63404c97-95bf-4d2c-859c-2b9c531b4018"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a406e80b-8576-4731-b478-4624330a8a03"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FirstName", "LastLoginAt", "LastName", "Phone", "Role" },
                values: new object[,]
                {
                    { new Guid("435ba206-442d-43a7-800e-a79bfbb0cdb4"), new DateTime(2026, 9, 9, 8, 20, 43, 140, DateTimeKind.Utc).AddTicks(4662), "Admin", new DateTime(2026, 9, 9, 8, 20, 43, 140, DateTimeKind.Utc).AddTicks(4662), "Test", "+79991234567", 0 },
                    { new Guid("fb76413c-5a89-40e0-b103-ecdf38cdc070"), new DateTime(2026, 9, 9, 8, 20, 43, 141, DateTimeKind.Utc).AddTicks(344), "Client", new DateTime(2026, 9, 9, 8, 20, 43, 141, DateTimeKind.Utc).AddTicks(344), "Test", "+79341234567", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Phone",
                table: "Users",
                column: "Phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Phone",
                table: "Users");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("435ba206-442d-43a7-800e-a79bfbb0cdb4"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("fb76413c-5a89-40e0-b103-ecdf38cdc070"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "FirstName", "LastLoginAt", "LastName", "Phone", "Role" },
                values: new object[,]
                {
                    { new Guid("63404c97-95bf-4d2c-859c-2b9c531b4018"), new DateTime(2026, 8, 25, 19, 47, 11, 354, DateTimeKind.Utc).AddTicks(8333), "Client", new DateTime(2026, 8, 25, 19, 47, 11, 354, DateTimeKind.Utc).AddTicks(8333), "Test", "+79341234567", 0 },
                    { new Guid("a406e80b-8576-4731-b478-4624330a8a03"), new DateTime(2026, 8, 25, 19, 47, 11, 354, DateTimeKind.Utc).AddTicks(3254), "Admin", new DateTime(2026, 8, 25, 19, 47, 11, 354, DateTimeKind.Utc).AddTicks(3254), "Test", "+79991234567", 0 }
                });
        }
    }
}
