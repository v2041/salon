using System.Net;
using System.Net.Http.Json;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using WebApiTests.Infrastructure;

namespace WebApiTests.Appointments;

[Collection(WebApiCollection.Name)]
public class ChangeAppointmentTimeTests(WebApiFixture fixture) : HttpTestBase(fixture)
{
    private static readonly DateOnly Day = DateOnly.FromDateTime(DateTime.Now).AddDays(3);
    private static readonly DateOnly NextDay = Day.AddDays(1);

    [Fact]
    public async Task ChangeTime_ToFreeSlotOnAnotherDay_MovesAppointment()
    {
        var admin = await SeedUserAsync(role: UserRole.Admin);
        var client = await SeedUserAsync("+79990000002");
        var schedule = await SeedScheduleAsync(Day);
        await SeedScheduleAsync(NextDay);
        var offering = await SeedOfferingAsync("Стрижка", minutes: 60);
        var appointment = await SeedAppointmentAsync(schedule.Id, client.Id, offering, new TimeOnly(10, 0));
        AuthenticateAs(admin);

        var response = await Client.PatchAsJsonAsync(
            $"/api/appointments/{appointment.Id}/change-time",
            new { id = appointment.Id, date = NextDay.ToString("yyyy-MM-dd"), time = "12:00:00" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var start = await WithDbAsync(async db =>
            (await db.Appointments.AsNoTracking().FirstAsync(a => a.Id == appointment.Id)).Interval.Start);
        Assert.Equal(new TimeOnly(12, 0), start);
    }

    [Fact]
    public async Task ChangeTime_ToBusySlot_Returns400AndKeepsOriginalSlot()
    {
        var admin = await SeedUserAsync(role: UserRole.Admin);
        var client = await SeedUserAsync("+79990000002");
        var schedule = await SeedScheduleAsync(Day);
        var targetSchedule = await SeedScheduleAsync(NextDay);
        var offering = await SeedOfferingAsync("Стрижка", minutes: 60);
        var appointment = await SeedAppointmentAsync(schedule.Id, client.Id, offering, new TimeOnly(10, 0));
        await SeedAppointmentAsync(targetSchedule.Id, client.Id, offering, new TimeOnly(11, 0));
        AuthenticateAs(admin);

        var response = await Client.PatchAsJsonAsync(
            $"/api/appointments/{appointment.Id}/change-time",
            new { id = appointment.Id, date = NextDay.ToString("yyyy-MM-dd"), time = "11:00:00" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var (date, start) = await WithDbAsync(async db =>
        {
            var stored = await db.Appointments
                .AsNoTracking()
                .Include(a => a.Schedule)
                .FirstAsync(a => a.Id == appointment.Id);
            return (stored.Schedule.Date, stored.Interval.Start);
        });
        Assert.Equal(Day, date);
        Assert.Equal(new TimeOnly(10, 0), start);
    }
}
