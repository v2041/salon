using Domain.Entities;
using Domain.Services;
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

                var settings = await db.SalonSettings.FirstOrDefaultAsync(token);
                if (settings is null)
                {
                    logger.LogWarning("Настройки не найдены. Генерация расписания пропущена.");
                }
                else
                {
                    await GenerateAsync(db, settings, token);
                }

                await Task.Delay(100000, token);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
            }
        }
    }

    private static async Task GenerateAsync(SalonDbContext db, SalonSettings settings, CancellationToken token)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var horizonEnd = today.AddDays(settings.HorizonDays);
        var schedules = await db.Schedules
            .Include(s => s.Appointments)
            .Where(s => s.Date > today && s.Date <= horizonEnd)
            .ToListAsync(token);

        var plan = SchedulePlanner.BuildGenerationPlan(
            settings,
            schedules.Select(ScheduleDayState.FromSchedule).ToList(),
            today);

        var created = SchedulePlanner.Apply(plan, schedules.ToDictionary(s => s.Date));
        db.Schedules.AddRange(created);
        await db.SaveChangesAsync(token);
    }
}
