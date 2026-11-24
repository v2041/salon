namespace Infrastructure.Authentication;

/// <summary>
/// Настройки VK ID. Секция конфига — <see cref="SectionName"/>.
/// </summary>
public class VkIdOptions
{
    public const string SectionName = "VkId";

    public string? AppId { get; set; }

    /// <summary>Сервисный ключ доступа. В обмене кода VK ждёт его в параметре service_token.</summary>
    public string? ServiceToken { get; set; }
}
