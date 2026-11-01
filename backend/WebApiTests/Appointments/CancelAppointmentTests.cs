using System.Net;
using System.Net.Http.Json;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApiTests.Infrastructure;

namespace WebApiTests.Appointments;

[Collection(WebApiCollection.Name)]
public class CancelAppointmentTests(WebApiFixture fixture) : HttpTestBase(fixture)
{
    private static readonly DateOnly Day = DateOnly.FromDateTime(DateTime.Now).AddDays(5);

    [Fact]
    public async Task Cancel_MoreThanTwoHoursAhead_ReturnsNoContent()
    {
        var (user, appointment) = await SeedOwnedAppointmentAsync();
        using var factory = CreateFactoryAt(Day.ToDateTime(new TimeOnly(9, 0)));
        using var client = CreateClientFor(factory, user);

        var response = await client.PatchAsJsonAsync(
            $"/api/appointments/{appointment.Id}/cancel",
            new { id = appointment.Id, status = AppointmentStatus.Canceled });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var status = await WithDbAsync(async db =>
            (await db.Appointments.AsNoTracking().FirstAsync(a => a.Id == appointment.Id)).Status);
        Assert.Equal(AppointmentStatus.Canceled, status);
    }

    [Fact]
    public async Task Cancel_LessThanTwoHoursAhead_Returns400()
    {
        var (user, appointment) = await SeedOwnedAppointmentAsync();
        using var factory = CreateFactoryAt(Day.ToDateTime(new TimeOnly(11, 0)));
        using var client = CreateClientFor(factory, user);

        var response = await client.PatchAsJsonAsync(
            $"/api/appointments/{appointment.Id}/cancel",
            new { id = appointment.Id, status = AppointmentStatus.Canceled });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Cancel_CompletedAppointment_Returns400()
    {
        var (user, appointment) = await SeedOwnedAppointmentAsync();
        await WithDbAsync(async db =>
        {
            var stored = await db.Appointments.FirstAsync(a => a.Id == appointment.Id);
            stored.Confirm();
            stored.Complete();
            await db.SaveChangesAsync();
        });
        using var factory = CreateFactoryAt(Day.ToDateTime(new TimeOnly(9, 0)));
        using var client = CreateClientFor(factory, user);

        var response = await client.PatchAsJsonAsync(
            $"/api/appointments/{appointment.Id}/cancel",
            new { id = appointment.Id, status = AppointmentStatus.Canceled });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<(User User, Appointment Appointment)> SeedOwnedAppointmentAsync()
    {
        var user = await SeedUserAsync("+79990000003");
        var schedule = await SeedScheduleAsync(Day);
        var offering = await SeedOfferingAsync("Стрижка", minutes: 60);
        var appointment = await SeedAppointmentAsync(schedule.Id, user.Id, offering, new TimeOnly(12, 0));
        return (user, appointment);
    }

    private WebApplicationFactory<Program> CreateFactoryAt(DateTime localNow)
        => Factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(new FixedTimeProvider(localNow));
            }));

    private HttpClient CreateClientFor(WebApplicationFactory<Program> factory, User user)
    {
        var client = factory.CreateClient();
        Authenticate(client, user);
        return client;
    }

    private sealed class FixedTimeProvider(DateTime localNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(localNow, TimeSpan.Zero);

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
