using System.Security.Cryptography;

namespace Infrastructure.Entities;

public class VerificationCode
{
    public const int MAX_ATTEMPTS = 5;
    public const int MAX_SENDS_PER_HOUR = 5;
    public const int MAX_ATTEMPTS_PER_HOUR = 20;

    private static readonly TimeSpan Window = TimeSpan.FromHours(1);

    public string Phone { get; private set; }
    public string Code { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int AttemptsCount { get; private set; }
    public int SendsInWindow { get; private set; }
    public DateTime SendWindowStart { get; private set; }
    public int AttemptsInWindow { get; private set; }
    public DateTime AttemptsWindowStart { get; private set; }
    public bool IsBlocked => AttemptsCount >= MAX_ATTEMPTS;

    private VerificationCode(
        string phone,
        string code,
        DateTime expiresAt,
        DateTime createdAt,
        int attemptsCount,
        int sendsInWindow,
        DateTime sendWindowStart,
        int attemptsInWindow,
        DateTime attemptsWindowStart)
    {
        Phone = phone;
        Code = code;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        AttemptsCount = attemptsCount;
        SendsInWindow = sendsInWindow;
        SendWindowStart = sendWindowStart;
        AttemptsInWindow = attemptsInWindow;
        AttemptsWindowStart = attemptsWindowStart;
    }

    public static VerificationCode Create(string phone)
    {
        var now = DateTime.UtcNow;
        return new VerificationCode(
            phone,
            GenerateCode(),
            now.AddMinutes(5),
            now,
            0,
            1,
            now,
            0,
            now);
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

    public bool TryResend(DateTime? now = null)
    {
        var current = now ?? DateTime.UtcNow;
        if (current - SendWindowStart >= Window)
        {
            SendWindowStart = current;
            SendsInWindow = 0;
        }

        if (SendsInWindow >= MAX_SENDS_PER_HOUR)
            return false;

        SendsInWindow++;
        CreatedAt = current;
        ExpiresAt = current.AddMinutes(5);
        Code = GenerateCode();
        AttemptsCount = 0;
        return true;
    }

    public bool TryRegisterAttempt(DateTime? now = null)
    {
        var current = now ?? DateTime.UtcNow;
        if (current - AttemptsWindowStart >= Window)
        {
            AttemptsWindowStart = current;
            AttemptsInWindow = 0;
        }

        if (AttemptsInWindow >= MAX_ATTEMPTS_PER_HOUR)
            return false;

        AttemptsInWindow++;
        return true;
    }
}
