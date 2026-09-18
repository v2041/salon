using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Features.Users.SendVerificationCode;

public static class Endpoint
{
    public static async Task<IResult> SendVerificationCodeAsync(
        SendVerificationCodeRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var existing = await db.VerificationCodes.FirstOrDefaultAsync(x => x.Phone == request.Phone, token);
        if (existing is not null)
            db.VerificationCodes.Remove(existing);

        var code = VerificationCode.Create(request.Phone);
        db.VerificationCodes.Add(code);
        await db.SaveChangesAsync(token);
        var response = new SendVerificationCodeResponse(code.ExpiresAt);
        return Results.Ok(response);
    }
}