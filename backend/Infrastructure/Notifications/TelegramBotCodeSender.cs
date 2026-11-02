using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Notifications;

/// <summary>
/// Отправляет код входа в Telegram. Без привязки номера возвращает ссылку на бота.
/// Сбой Bot API — исключение.
/// </summary>
public class TelegramBotCodeSender(
    SalonDbContext db,
    TelegramBotClient client,
    IOptions<TelegramOptions> options) : ICodeSender
{
    public async Task<CodeSendResult> SendAsync(string phone, string code, CancellationToken cancellationToken)
    {
        var binding = await db.TelegramBindings
            .FirstOrDefaultAsync(x => x.Phone == phone, cancellationToken);
        if (binding is null)
        {
            var username = options.Value.Username;
            var bindingUrl = string.IsNullOrWhiteSpace(username) ? null : $"https://t.me/{username}";
            return new CodeSendResult(false, bindingUrl);
        }

        await client.SendMessageAsync(
            binding.ChatId,
            $"Код для входа: {code}. Никому не сообщайте.",
            cancellationToken);
        return new CodeSendResult(true, null);
    }
}
