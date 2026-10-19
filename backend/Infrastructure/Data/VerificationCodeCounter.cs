using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public static class VerificationCodeCounter
{
    public static async Task<bool> TryCountFailedAttemptAsync(
        DbSet<VerificationCode> codes,
        string phone,
        CancellationToken token
    )
    {
        var updated = await codes
            .Where(x => x.Phone == phone && x.AttemptsCount < VerificationCode.MAX_ATTEMPTS)
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.AttemptsCount, v => v.AttemptsCount + 1), token);
        return updated > 0;
    }
}
