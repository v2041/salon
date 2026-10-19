using System.Security.Cryptography;

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

    public static VerificationCode Create(string phone)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(5);
        var code = GenerateCode();
        return new VerificationCode(phone, code, expiresAt, now);
    }

    private static string GenerateCode() => RandomNumberGenerator.GetInt32(1000, 10000).ToString();

    public void RegenerateCode()
    {
        var now = DateTime.UtcNow;
        if (CreatedAt.AddMinutes(1) >= now) return;
        CreatedAt = now;
        ExpiresAt = now.AddMinutes(5);
        Code = GenerateCode();
    }
}
