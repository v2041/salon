namespace Infrastructure.Entities;

public class VerificationCode
{
    public string Phone { get; private set; }
    public string Code { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private VerificationCode(string phone, string code, DateTime expiresAt, DateTime createdAt)
    {
        Phone = phone;
        Code = code;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public static VerificationCode Create(string phone, DateTime? dateTime = null)
    {
        var now = dateTime ?? DateTime.UtcNow;
        var code = GenerateCode();
        var expiresAt = now.AddMinutes(5);
        return new VerificationCode(phone, code, expiresAt, now);
    }
    
    private static string GenerateCode() => Random.Shared.Next(1000, 9999).ToString();
}