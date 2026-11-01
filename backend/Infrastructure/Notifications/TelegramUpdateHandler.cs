using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Notifications;

/// <summary>
/// Обрабатывает обновления Telegram без HTTP: /start, контакт, прочее сообщение.
/// Контакт принимается, только если он принадлежит отправителю.
/// Повторный контакт обновляет chat_id (привязка идемпотентна).
/// </summary>
public class TelegramUpdateHandler(SalonDbContext db, ITelegramMessageSender sender)
{
    private const string StartText =
        "Здравствуйте! Нажмите «Поделиться номером», чтобы привязать номер телефона.";
    private const string HintText =
        "Нажмите «Поделиться номером», чтобы привязать номер телефона.";
    private const string BoundText =
        "Номер привязан. Вернитесь на сайт и нажмите «Получить код».";
    private const string ShareContactButtonText = "Поделиться номером";

    public async Task HandleAsync(TelegramUpdate update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message) return;

        if (message.Contact is { } contact)
        {
            await AcceptContactAsync(message, contact, cancellationToken);
            return;
        }

        var text = message.Text == "/start" ? StartText : HintText;
        await sender.SendMessageAsync(message.ChatId, text, ContactKeyboard(), cancellationToken);
    }

    private async Task AcceptContactAsync(
        TelegramMessage message,
        TelegramContact contact,
        CancellationToken cancellationToken)
    {
        if (message.FromId is null || contact.UserId != message.FromId) return;
        if (NormalizePhone(contact.PhoneNumber) is not { } phone) return;

        var binding = await db.TelegramBindings
            .FirstOrDefaultAsync(x => x.Phone == phone, cancellationToken);
        if (binding is null)
        {
            db.TelegramBindings.Add(TelegramBinding.Create(phone, message.ChatId));
        }
        else
        {
            binding.Rebind(message.ChatId);
        }
        await db.SaveChangesAsync(cancellationToken);

        await sender.SendMessageAsync(message.ChatId, BoundText, null, cancellationToken);
    }

    private static string? NormalizePhone(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (digits.Length == 11 && digits[0] == '8') digits = "7" + digits[1..];
        return digits.Length == 11 && digits[0] == '7' ? "+" + digits : null;
    }

    private static object ContactKeyboard() => new
    {
        keyboard = new[]
        {
            new[] { new { text = ShareContactButtonText, request_contact = true } }
        }
    };
}
