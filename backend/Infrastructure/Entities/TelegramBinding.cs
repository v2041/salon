namespace Infrastructure.Entities;

public class TelegramBinding
{
    public string Phone { get; private set; }
    public long ChatId { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TelegramBinding(string phone, long chatId, DateTime updatedAt)
    {
        Phone = phone;
        ChatId = chatId;
        UpdatedAt = updatedAt;
    }

    public static TelegramBinding Create(string phone, long chatId)
    {
        return new TelegramBinding(phone, chatId, DateTime.UtcNow);
    }

    public void Rebind(long chatId, DateTime? now = null)
    {
        ChatId = chatId;
        UpdatedAt = now ?? DateTime.UtcNow;
    }
}
