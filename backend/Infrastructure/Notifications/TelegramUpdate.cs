namespace Infrastructure.Notifications;

/// <summary>Обновление из Bot API.</summary>
public sealed record TelegramUpdate(long UpdateId, TelegramMessage? Message);

/// <summary>Сообщение обновления.</summary>
public sealed record TelegramMessage(long ChatId, long? FromId, string? Text, TelegramContact? Contact);

/// <summary>Контакт из кнопки «Поделиться номером».</summary>
public sealed record TelegramContact(string PhoneNumber, long? UserId);
