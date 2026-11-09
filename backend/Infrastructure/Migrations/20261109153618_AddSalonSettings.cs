using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalonSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "Schedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SalonSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    WorkStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    WorkEndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    BreakStartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    BreakEndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    GenerationEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    HorizonDays = table.Column<int>(type: "integer", nullable: false),
                    WeekendDays = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalonSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SalonSettings",
                columns: new[] { "Id", "Address", "GenerationEnabled", "HorizonDays", "Phone", "WeekendDays", "BreakEndTime", "BreakStartTime", "WorkEndTime", "WorkStartTime" },
                values: new object[] { new Guid("7f2b9d64-3c1a-4e58-9b0d-6a5f4e3d2c1b"), "ул. Примерная, д. 1", true, 14, "+79991234567", new[] { 0 }, new TimeOnly(15, 20, 0), new TimeOnly(15, 0, 0), new TimeOnly(18, 0, 0), new TimeOnly(10, 0, 0) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalonSettings");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Schedules");
        }
    }
}
