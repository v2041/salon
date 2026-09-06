using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.BackgroundServices;

public class ScheduleService(IServiceScopeFactory scopeFactory, ILogger<ScheduleService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        logger.LogInformation("Служба ScheduleGenerateService запущена в {Time}", DateTimeOffset.Now);
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SalonDbContext>();

                var today = DateOnly.FromDateTime(DateTime.Now);
                var createBound = today.AddDays(14);

                var schedules = await db.Schedules
                    .Where(s => s.Date >= today && s.Date <= createBound)
                    .ToListAsync(token);

                List<Schedule> newSchedules = [];
                for (var i = 1; i < 14; i++)
                {
                    var date = today.AddDays(i);
                    if (schedules.Any(s => s.Date == date)) continue;
                    var newSchedule = Schedule.Create(
                        date,
                        date.DayOfWeek != DayOfWeek.Sunday,
                        TimeInterval.Create(TimeOnly.Parse("10:00"), TimeOnly.Parse("18:00")),
                        TimeInterval.Create(TimeOnly.Parse("15:00"), TimeOnly.Parse("15:20"))
                    );
                    newSchedules.Add(newSchedule);
                }

                db.Schedules.AddRange(newSchedules);
                await db.SaveChangesAsync(token);

                await Task.Delay(100000, token);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
            }
        }
    }
}