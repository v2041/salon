using System.Security.Cryptography;

namespace Infrastructure.Entities;

public class VerificationCode
{
    public const int MAX_ATTEMPTS = 5;

    public string Phone { get; private set; }
    public string Code { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int AttemptsCount { get; private set; }
    public bool IsBlocked => AttemptsCount >= MAX_ATTEMPTS;

    private VerificationCode(string phone, string code, DateTime expiresAt, DateTime createdAt, int attemptsCount)
    {
        Phone = phone;
        Code = code;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        AttemptsCount = attemptsCount;
    }

    public static VerificationCode Create(string phone)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(5);
        var code = GenerateCode();
        return new VerificationCode(phone, code, expiresAt, now, 0);
    }

    private static string GenerateCode() => RandomNumberGenerator.GetInt32(1000, 10000).ToString();

    public VerificationResult Verify(string code, DateTime? now = null)
    {
        var current = now ?? DateTime.UtcNow;
        if (ExpiresAt <= current) return VerificationResult.Expired;
        if (IsBlocked) return VerificationResult.Blocked;
        if (Code == code) return VerificationResult.Valid;
        AttemptsCount++;
        return VerificationResult.Invalid;
    }

    public void RegenerateCode(DateTime? now = null)
    {
        var current = now ?? DateTime.UtcNow;
        if (CreatedAt.AddMinutes(1) > current) return;
        CreatedAt = current;
        ExpiresAt = current.AddMinutes(5);
        Code = GenerateCode();
        AttemptsCount = 0;
    }
}
