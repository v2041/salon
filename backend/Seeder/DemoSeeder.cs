using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Seeder;

public static class DemoSeeder
{
    private const string AdminPhone = "+79991234567";
    private const string ClientPhone = "+79341234567";
    private const int ClientCount = 60;
    private const int DaysBack = 120;
    private const int DaysAhead = 21;

    private static readonly string[] FirstNames =
    [
        "Анна", "Мария", "Елена", "Ольга", "Наталья", "Ирина", "Светлана", "Татьяна",
        "Екатерина", "Юлия", "Ксения", "Алина", "Виктория", "Полина", "Дарья", "София",
        "Александра", "Вероника", "Марина", "Людмила", "Валентина", "Галина", "Евгения", "Кристина"
    ];

    private static readonly string[] LastNames =
    [
        "Иванова", "Петрова", "Смирнова", "Кузнецова", "Соколова", "Попова", "Лебедева",
        "Козлова", "Новикова", "Морозова", "Волкова", "Алексеева", "Павлова", "Семёнова",
        "Голубева", "Виноградова", "Богданова", "Воробьёва", "Фёдорова", "Михайлова"
    ];

    private static readonly OfferingSeed[] OfferingCatalog =
    [
        new("Стрижка женская", "Стрижка, мытьё и укладка феном.", 2500, 60, Category.Hair),
        new("Маникюр с покрытием", "Маникюр, база, гель-лак и топ.", 2300, 90, Category.Nails),
        new("Окрашивание в один тон", "Окрашивание корней и полотна.", 4500, 120, Category.Hair),
        new("Маникюр классический", "Маникюр без покрытия и дизайна.", 1500, 60, Category.Nails),
        new("Чистка лица", "Глубокая чистка с уходом по типу кожи.", 3200, 90, Category.Face),
        new("Стрижка мужская", "Стрижка машинкой и ножницами.", 1800, 45, Category.Hair),
        new("Педикюр", "Педикюр с обработкой стоп.", 2800, 90, Category.Nails),
        new("Балаяж", "Осветление с плавным переходом.", 6500, 180, Category.Hair),
        new("Наращивание ногтей", "Гель на форму, коррекция не входит.", 3500, 120, Category.Nails),
        new("Макияж вечерний", "Макияж для мероприятия.", 3500, 90, Category.Other),
        new("Укладка вечерняя", "Локоны или гладкая укладка.", 2200, 60, Category.Hair),
        new("Пилинг", "Химический пилинг для обновления кожи.", 2800, 60, Category.Face),
        new("Снятие покрытия", "Снятие гель-лака без маникюра.", 800, 30, Category.Nails),
        new("Макияж дневной", "Естественный макияж на день.", 2500, 60, Category.Other),
        new("Уход Olaplex", "Восстановление волос по протоколу.", 3000, 45, Category.Hair),
        new("Массаж лица", "Лимфодренажный массаж лица.", 2000, 45, Category.Face),
        new("Ламинирование бровей", "Укладка и окрашивание бровей.", 1800, 45, Category.Face),
        new("Депиляция воском", "Восковая депиляция зоны.", 1200, 30, Category.Other),
        new("Окрашивание бровей", "Коррекция формы и цвета.", 900, 30, Category.Face),
        new("Стрижка детская", "Стрижка для ребёнка до 12 лет.", 1200, 45, Category.Hair, false),
        new("Парафинотерапия", "Прогревание и уход для рук.", 1400, 40, Category.Other, false),
        new("Ламинирование ресниц", "Уход и завиток ресниц.", 2900, 90, Category.Face, false)
    ];

    public static async Task SeedAsync(SalonDbContext db, bool reset, CancellationToken token)
    {
        await db.Database.MigrateAsync(token);
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(20261020)", token);

        if (reset)
        {
            await ResetAsync(db, token);
        }
        else if (await db.Offerings.AnyAsync(token)
                 || await db.Schedules.AnyAsync(token)
                 || await db.Users.CountAsync(token) > 2)
        {
            throw new InvalidOperationException("В базе уже есть данные. Запустите с --reset.");
        }

        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var random = new Random(20261020);
        var clients = BuildClients();
        var offerings = BuildOfferings();
        var schedules = BuildSchedules(today, random);
        BuildAppointments(schedules, clients, offerings, today, now, random);

        db.Users.AddRange(clients);
        db.Offerings.AddRange(offerings);
        db.Schedules.AddRange(schedules);

        await db.SaveChangesAsync(token);

        await PrintSummaryAsync(db, token);
        await transaction.CommitAsync(token);
        Console.WriteLine("Готово.");
    }

    private static async Task ResetAsync(SalonDbContext db, CancellationToken token)
    {
        await db.Set<AppointmentOffering>().ExecuteDeleteAsync(token);
        await db.Appointments.ExecuteDeleteAsync(token);
        await db.Offerings.ExecuteDeleteAsync(token);
        await db.Schedules.ExecuteDeleteAsync(token);
        await db.Users
            .Where(u => u.Phone != AdminPhone && u.Phone != ClientPhone)
            .ExecuteDeleteAsync(token);
        await db.VerificationCodes.ExecuteDeleteAsync(token);
    }

    private static List<User> BuildClients()
    {
        var result = new List<User>(ClientCount);
        for (var i = 0; i < ClientCount; i++)
        {
            var firstName = FirstNames[i % FirstNames.Length];
            var lastName = LastNames[(i * 7 + 3) % LastNames.Length];
            string? optionalLastName = i % 11 == 0 ? null : lastName;
            var phone = $"+7916{1000000 + i:D7}";
            var createdAt = DateTime.UtcNow.AddDays(-(14 + i * 4 % 240)).AddHours(-(i % 9));
            result.Add(User.Create(firstName, optionalLastName, phone, createdAt));
        }
        return result;
    }

    private static List<Offering> BuildOfferings()
    {
        return OfferingCatalog
            .Select(seed =>
            {
                var offering = Offering.Create(
                    Money.FromDecimal(seed.Price),
                    seed.Title,
                    seed.Description,
                    TimeSpan.FromMinutes(seed.Minutes),
                    seed.Category);
                if (!seed.Active)
                    offering.Remove();
                return offering;
            })
            .ToList();
    }

    private static List<Schedule> BuildSchedules(DateOnly today, Random random)
    {
        var result = new List<Schedule>(DaysBack + DaysAhead + 1);
        for (var offset = -DaysBack; offset <= DaysAhead; offset++)
        {
            var date = today.AddDays(offset);
            var sunday = date.DayOfWeek == DayOfWeek.Sunday;
            var vacation = !sunday && offset != 0 && random.NextDouble() < 0.07;
            var working = !sunday && !vacation;
            var (work, pause) = WorkHours(random);
            result.Add(Schedule.Create(date, working, work, pause, checkPast: false));
        }
        return result;
    }

    private static (TimeInterval Work, TimeInterval? Break) WorkHours(Random random)
    {
        switch (random.Next(10))
        {
            case 0:
                return (Interval("09:00", "19:00"), Interval("13:00", "14:00"));
            case 1:
                return (Interval("11:00", "17:00"), null);
            case 2:
                return (Interval("10:00", "20:00"), Interval("14:00", "14:30"));
            case 3:
                return (Interval("10:00", "18:00"), null);
            default:
                return (Interval("10:00", "18:00"), Interval("15:00", "15:20"));
        }
    }

    private static TimeInterval Interval(string start, string end)
        => TimeInterval.Create(TimeOnly.Parse(start), TimeOnly.Parse(end));

    private static void BuildAppointments(
        List<Schedule> schedules,
        List<User> clients,
        List<Offering> offerings,
        DateOnly today,
        DateTime now,
        Random random)
    {
        var active = offerings.Where(o => o.IsActive).ToList();
        var earliestToday = now.AddMinutes(30);
        TimeOnly? todayStart = DateOnly.FromDateTime(earliestToday) == today
            ? TimeOnly.FromDateTime(earliestToday)
            : null;

        foreach (var schedule in schedules)
        {
            var offset = schedule.Date.DayNumber - today.DayNumber;
            if (!schedule.IsWorking)
                continue;

            var target = offset switch
            {
                < 0 => random.Next(4, 10),
                0 => todayStart is null ? 0 : random.Next(2, 5),
                _ => random.Next(2, 7)
            };
            if (offset > 0 && random.NextDouble() < 0.12)
                target = 0;

            var usedClients = new HashSet<Guid>();
            var start = schedule.WorkInterval.Start.AddMinutes(random.Next(0, 25));
            if (offset == 0 && todayStart is not null)
            {
                start = todayStart.Value > schedule.WorkInterval.Start
                    ? todayStart.Value
                    : schedule.WorkInterval.Start;
            }
            start = RoundToFive(start);

            for (var index = 0; index < target; index++)
            {
                var chosen = PickOfferings(active, random);
                var duration = TimeSpan.FromMinutes(chosen.Sum(o => o.Duration.TotalMinutes));
                var placed = PlaceStart(schedule, start, duration);
                if (placed is null)
                    break;

                var client = PickClient(clients, usedClients, random);
                var data = chosen
                    .Select(o => new AppointmentOfferingData(
                        o.Id,
                        Money.FromDecimal(o.Price.Value, o.Price.Currency),
                        o.Duration))
                    .ToList();
                var appointment = schedule.AddAppointment(
                    client.Id,
                    placed.Value,
                    data,
                    checkPast: false);
                ApplyStatus(appointment, offset, random);
                usedClients.Add(client.Id);
                start = RoundToFive(appointment.Interval.End.AddMinutes(random.Next(5, 36)));
            }
        }
    }

    private static TimeOnly? PlaceStart(Schedule schedule, TimeOnly start, TimeSpan duration)
    {
        var current = start;
        for (var attempt = 0; attempt < 4; attempt++)
        {
            if (current < schedule.WorkInterval.Start
                || current > schedule.WorkInterval.End
                || duration > schedule.WorkInterval.End - current)
            {
                return null;
            }
            var end = current.Add(duration);
            if (schedule.BreakInterval is null
                || !TimeInterval.Create(current, end).IsOverlapping(schedule.BreakInterval))
            {
                return current;
            }
            current = schedule.BreakInterval.End;
        }
        return null;
    }

    private static List<Offering> PickOfferings(List<Offering> offerings, Random random)
    {
        var count = random.NextDouble() < 0.72 ? 1 : 2;
        var result = new List<Offering>(count);
        for (var i = 0; i < count; i++)
        {
            var index = (int)(Math.Pow(random.NextDouble(), 2) * offerings.Count);
            var pick = offerings[Math.Min(index, offerings.Count - 1)];
            if (result.All(o => o.Id != pick.Id))
                result.Add(pick);
        }
        return result;
    }

    private static User PickClient(List<User> clients, HashSet<Guid> used, Random random)
    {
        var available = clients.Where(c => !used.Contains(c.Id)).ToList();
        if (available.Count == 0)
            available = clients;
        var regularCount = Math.Max(3, available.Count / 5);
        var index = random.NextDouble() < 0.65
            ? random.Next(Math.Min(regularCount, available.Count))
            : random.Next(available.Count);
        return available[index];
    }

    private static void ApplyStatus(Appointment appointment, int offset, Random random)
    {
        if (offset < 0)
        {
            var roll = random.NextDouble();
            if (roll < 0.82)
            {
                appointment.Confirm();
                appointment.Complete();
            }
            else if (roll < 0.89)
            {
                appointment.Cancel();
            }
            else if (roll < 0.95)
            {
                appointment.Confirm();
                appointment.Miss();
            }
            else
            {
                appointment.Reject();
            }
            return;
        }

        if (random.NextDouble() < (offset == 0 ? 0.5 : 0.4))
            appointment.Confirm();
    }

    private static TimeOnly RoundToFive(TimeOnly time)
    {
        var total = time.Hour * 60 + time.Minute;
        var rounded = (total + 4) / 5 * 5;
        return new TimeOnly(rounded / 60 % 24, rounded % 60);
    }

    private static async Task PrintSummaryAsync(SalonDbContext db, CancellationToken token)
    {
        var users = await db.Users.CountAsync(token);
        var offerings = await db.Offerings.CountAsync(token);
        var activeOfferings = await db.Offerings.CountAsync(o => o.IsActive, token);
        var schedules = await db.Schedules.CountAsync(token);
        var appointments = await db.Appointments.CountAsync(token);
        var byStatus = await db.Appointments
            .GroupBy(a => a.Status)
            .Select(g => new {Status = g.Key, Count = g.Count()})
            .ToListAsync(token);

        Console.WriteLine(
            $"Демо-данные подготовлены: пользователей {users}, услуг {offerings} (активных {activeOfferings}), " +
            $"дней {schedules}, записей {appointments}.");
        foreach (var row in byStatus.OrderBy(r => (int)r.Status))
            Console.WriteLine($"  {StatusLabel(row.Status)}: {row.Count}");
    }

    private static string StatusLabel(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Pending => "ожидают",
        AppointmentStatus.Confirmed => "подтверждены",
        AppointmentStatus.Completed => "завершены",
        AppointmentStatus.Canceled => "отменены",
        AppointmentStatus.Missed => "пропущены",
        AppointmentStatus.Rejected => "отклонены",
        _ => status.ToString()
    };

    private sealed record OfferingSeed(
        string Title,
        string Description,
        decimal Price,
        int Minutes,
        Category Category,
        bool Active = true);
}
