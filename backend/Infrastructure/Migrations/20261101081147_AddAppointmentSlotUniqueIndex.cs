using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentSlotUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"UX_Appointments_ScheduleId_StartTime\" " +
                "ON \"Appointments\" (\"ScheduleId\", \"StartTime\") " +
                "WHERE \"Status\" IN (0, 1);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"UX_Appointments_ScheduleId_StartTime\";");
        }
    }
}
