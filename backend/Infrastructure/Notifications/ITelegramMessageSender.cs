namespace Infrastructure.Notifications;

/// <summary>Отправка сообщений в чат Telegram.</summary>
public interface ITelegramMessageSender
{
    Task SendMessageAsync(
        long chatId,
        string text,
        object? replyMarkup,
        CancellationToken cancellationToken);
}
