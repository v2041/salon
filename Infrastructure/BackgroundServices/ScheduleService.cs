using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.BackgroundServices;

public class ScheduleService(IServiceScopeFactory scopeFactory, ILogger<ScheduleService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        logger.LogInformation("Служба ScheduleGenerateService запущена в {Time}", DateTimeOffset.UtcNow);
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SalonDbContext>();
                
                var today = DateOnly.FromDateTime(DateTime.Now);
                var tomorrow = today.AddDays(1);
                
                var schedule = await db.Schedules
                    .FirstOrDefaultAsync(s => s.Date == tomorrow, token);
                if (schedule is null)
                {
                    var newSchedule = Schedule.Create(
                        tomorrow,
                        tomorrow.DayOfWeek != DayOfWeek.Sunday,
                        TimeInterval.Create(TimeOnly.Parse("10:00"), TimeOnly.Parse("18:00")),
                        TimeInterval.Create(TimeOnly.Parse("15:00"), TimeOnly.Parse("15:20"))
                    );
                    db.Schedules.Add(newSchedule);
                    await db.SaveChangesAsync(token);
                }

                await db.Schedules
                    .Where(s => s.Date < today)
                    .ExecuteDeleteAsync(token);

                await Task.Delay(100000, token);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
            }
        }
    }
}