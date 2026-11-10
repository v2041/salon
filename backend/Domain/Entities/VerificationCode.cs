using System.Security.Cryptography;
using Domain.Enums;

namespace Domain.Entities;

public class VerificationCode
{
    public const int MAX_ATTEMPTS = 5;
    public const int CODE_TTL_MINUTES = 5;
    public const int RESEND_COOLDOWN_MINUTES = 1;

    public string Phone { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int AttemptsCount { get; private set; }
    public bool IsBlocked => AttemptsCount >= MAX_ATTEMPTS;

    private VerificationCode()
    {
    }

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
        phone = NormalizePhone(phone);
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(CODE_TTL_MINUTES);
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

    public bool RegenerateCode(DateTime? now = null)
    {
        var current = now ?? DateTime.UtcNow;
        if (CreatedAt.AddMinutes(RESEND_COOLDOWN_MINUTES) > current) return false;
        CreatedAt = current;
        ExpiresAt = current.AddMinutes(CODE_TTL_MINUTES);
        Code = GenerateCode();
        AttemptsCount = 0;
        return true;
    }
    public static string NormalizePhone(string phone)
    {
        return phone.StartsWith('8') ? string.Concat("+7", phone.AsSpan(1)) : phone;
    }
}
