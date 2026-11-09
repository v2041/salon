using Infrastructure.Data;
using Infrastructure.Services;
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
                var runner = scope.ServiceProvider.GetRequiredService<ScheduleGenerationRunner>();

                var settings = await db.SalonSettings.FirstOrDefaultAsync(token);
                if (settings is null)
                {
                    logger.LogWarning("Настройки не найдены. Генерация расписания пропущена.");
                }
                else
                {
                    await runner.GenerateAsync(settings, token);
                }

                await Task.Delay(100000, token);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
            }
        }
    }
}
