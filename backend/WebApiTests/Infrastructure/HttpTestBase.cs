using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApiTests.Infrastructure;

public abstract class HttpTestBase(WebApiFixture fixture) : IAsyncLifetime
{
    protected WebApiFactory Factory => fixture.Factory;

    protected HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await fixture.ResetDatabaseAsync();
        Client = Factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        Client?.Dispose();
        return Task.CompletedTask;
    }

    protected async Task<T> WithDbAsync<T>(Func<SalonDbContext, Task<T>> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalonDbContext>();
        return await action(db);
    }

    protected async Task WithDbAsync(Func<SalonDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalonDbContext>();
        await action(db);
    }

    protected Task<User> SeedUserAsync(string phone = "+79990000001", UserRole role = UserRole.Default)
    {
        var user = User.Create("Тест", "Клиент", phone, DateTime.UtcNow);
        if (role == UserRole.Admin)
            user.PromoteToAdmin();

        return WithDbAsync(async db =>
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        });
    }

    protected void AuthenticateAs(User user)
    {
        var configuration = Factory.Services.GetRequiredService<IConfiguration>();
        var token = JwtTokenGenerator.Create(user, configuration);
        Client.DefaultRequestHeaders.Remove("Cookie");
        Client.DefaultRequestHeaders.Add("Cookie", $"some-cookies={token}");
    }

    protected Task<Offering> SeedOfferingAsync(
        string title,
        int minutes = 60,
        decimal price = 1000m,
        bool isActive = true)
    {
        var offering = Offering.Create(
            Money.FromDecimal(price),
            title,
            "Описание услуги",
            TimeSpan.FromMinutes(minutes),
            Category.Hair);
        if (!isActive)
            offering.Remove();

        return WithDbAsync(async db =>
        {
            db.Offerings.Add(offering);
            await db.SaveChangesAsync();
            return offering;
        });
    }

    protected Task<Schedule> SeedScheduleAsync(DateOnly date, bool isWorking = true)
    {
        var schedule = Schedule.Create(
            date,
            isWorking,
            TimeInterval.Create(new TimeOnly(10, 0), new TimeOnly(18, 0)),
            null,
            checkPast: false);

        return WithDbAsync(async db =>
        {
            db.Schedules.Add(schedule);
            await db.SaveChangesAsync();
            return schedule;
        });
    }

    protected Task<Appointment> SeedAppointmentAsync(
        Guid scheduleId,
        Guid userId,
        Offering offering,
        TimeOnly start)
    {
        return WithDbAsync(async db =>
        {
            var schedule = await db.Schedules
                .Include(s => s.Appointments)
                .FirstAsync(s => s.Id == scheduleId);
            var data = new List<AppointmentOfferingData>
            {
                new(offering.Id, offering.Price, offering.Duration)
            };
            var appointment = schedule.AddAppointment(userId, start, data, checkPast: false);
            await db.SaveChangesAsync();
            return appointment;
        });
    }
}
