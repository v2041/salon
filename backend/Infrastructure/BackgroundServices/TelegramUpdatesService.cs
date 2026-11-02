using Infrastructure.Notifications;

namespace Infrastructure.BackgroundServices;

/// <summary>
/// Принимает обновления Telegram long polling (getUpdates, timeout 50 секунд)
/// и передаёт их <see cref="TelegramUpdateHandler"/>.
/// Ошибка Bot API или обработки не останавливает сервис.
/// </summary>
public class TelegramUpdatesService(
    IServiceScopeFactory scopeFactory,
    ILogger<TelegramUpdatesService> logger) : BackgroundService
{
    private static readonly TimeSpan ErrorDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        logger.LogInformation("Служба приёма обновлений Telegram запущена");
        long? offset = null;
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<TelegramBotClient>();
                var handler = scope.ServiceProvider.GetRequiredService<TelegramUpdateHandler>();

                var updates = await client.GetUpdatesAsync(offset, token);
                foreach (var update in updates)
                {
                    await handler.HandleAsync(update, token);
                    offset = update.UpdateId + 1;
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                await Task.Delay(ErrorDelay, token);
            }
        }
    }
}
