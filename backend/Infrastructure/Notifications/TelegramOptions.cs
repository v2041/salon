namespace Infrastructure.Notifications;

/// <summary>
/// Настройки бота салона. Секция конфига — <see cref="SectionName"/>.
/// </summary>
public class TelegramOptions
{
    public const string SectionName = "Telegram:Bot";

    public string? Token { get; set; }
    public string? Username { get; set; }
}
