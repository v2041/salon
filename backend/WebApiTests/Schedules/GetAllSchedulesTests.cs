using System.Net;
using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApiTests.Infrastructure;

namespace WebApiTests.Schedules;

[Collection(WebApiCollection.Name)]
public class GetAllSchedulesTests(WebApiFixture fixture) : HttpTestBase(fixture)
{
    private static readonly DateOnly Day = DateOnly.FromDateTime(DateTime.Now).AddDays(3);

    [Fact]
    public async Task GetAllSchedules_WithAppointment_ReturnsDayWithAppointment()
    {
        var (scheduleId, appointmentId, userId) = await SeedDayWithAppointmentAsync();
        await AuthenticateAsAdminAsync();

        var response = await Client.GetAsync("/api/schedules/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var day = Assert.Single(json.RootElement.EnumerateArray(), d => d.GetProperty("id").GetGuid() == scheduleId);
        var appointments = day.GetProperty("appointments");
        Assert.Equal(JsonValueKind.Array, appointments.ValueKind);
        var appointment = Assert.Single(appointments.EnumerateArray());
        Assert.Equal(appointmentId, appointment.GetProperty("id").GetGuid());
        Assert.Equal(userId, appointment.GetProperty("userId").GetGuid());
        Assert.Equal("Иван Петров", appointment.GetProperty("userName").GetString());
        Assert.Equal("10:00:00", appointment.GetProperty("interval").GetProperty("start").GetString());
        Assert.Equal("10:30:00", appointment.GetProperty("interval").GetProperty("end").GetString());
        Assert.Equal((int)AppointmentStatus.Pending, appointment.GetProperty("status").GetInt32());
        Assert.Equal(1500m, appointment.GetProperty("price").GetProperty("value").GetDecimal());
        Assert.Equal("RUB", appointment.GetProperty("price").GetProperty("currency").GetString());
    }

    [Fact]
    public async Task GetAllSchedules_WithoutAppointments_ReturnsEmptyAppointments()
    {
        var scheduleId = await SeedDayAsync();
        await AuthenticateAsAdminAsync();

        var response = await Client.GetAsync("/api/schedules/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var day = Assert.Single(json.RootElement.EnumerateArray(), d => d.GetProperty("id").GetGuid() == scheduleId);
        var appointments = day.GetProperty("appointments");
        Assert.Equal(JsonValueKind.Array, appointments.ValueKind);
        Assert.Equal(0, appointments.GetArrayLength());
    }

    private async Task<Guid> SeedDayAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalonDbContext>();
        var schedule = Schedule.Create(
            Day,
            true,
            TimeInterval.Create(new TimeOnly(10, 0), new TimeOnly(20, 0)),
            null);
        db.Schedules.Add(schedule);
        await db.SaveChangesAsync();
        return schedule.Id;
    }

    private async Task<(Guid ScheduleId, Guid AppointmentId, Guid UserId)> SeedDayWithAppointmentAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalonDbContext>();
        var user = User.Create("Иван", "Петров", "+79990000001", DateTime.UtcNow);
        var offering = Offering.Create(
            Money.FromDecimal(1500),
            "Стрижка",
            "Классическая стрижка",
            TimeSpan.FromMinutes(30),
            Category.Hair);
        var schedule = Schedule.Create(
            Day,
            true,
            TimeInterval.Create(new TimeOnly(10, 0), new TimeOnly(20, 0)),
            null);
        var appointment = schedule.AddAppointment(
            user.Id,
            new TimeOnly(10, 0),
            [new AppointmentOfferingData(offering.Id, offering.Price, offering.Duration)]);
        db.Users.Add(user);
        db.Offerings.Add(offering);
        db.Schedules.Add(schedule);
        await db.SaveChangesAsync();
        return (schedule.Id, appointment.Id, user.Id);
    }

    private async Task AuthenticateAsAdminAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalonDbContext>();
        var admin = User.Create("Админ", "Тест", "+79990000000", DateTime.UtcNow);
        admin.PromoteToAdmin();
        db.Users.Add(admin);
        await db.SaveChangesAsync();
        var configuration = Factory.Services.GetRequiredService<IConfiguration>();
        var token = JwtTokenGenerator.Create(admin, configuration);
        Client.DefaultRequestHeaders.Add("Cookie", $"some-cookies={token}");
    }
}
