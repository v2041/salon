using Infrastructure.Authentication;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Users.Login;

public static class Endpoint
{
    public static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        SalonDbContext db,
        IConfiguration configuration,
        HttpContext context,
        CancellationToken token
    )
    {
        var code = await db.VerificationCodes.FirstOrDefaultAsync(x =>
            x.Phone == request.Phone, token);
        if (code == null)
            return Results.BadRequest("Неверный номер телефона или код");
        var result = code.Verify(request.Code);
        if (result == VerificationResult.Invalid
            && !await VerificationCodeCounter.TryCountFailedAttemptAsync(db.VerificationCodes, request.Phone, token))
        {
            result = VerificationResult.Blocked;
        }
        if (result != VerificationResult.Valid)
        {
            return Results.BadRequest(result switch
            {
                VerificationResult.Blocked => "Вы превысили допустимое число попыток. Запросите новый код",
                VerificationResult.Expired => "Код истёк. Запросите новый код",
                _ => "Неверный номер телефона или код"
            });
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone, token);
        if (user == null)
            return Results.BadRequest("Пользователь не найден");
        db.VerificationCodes.Remove(code);

        user.MarkLoggedIn();
        await db.SaveChangesAsync(token);
        var jwtToken = JwtTokenGenerator.Create(user, configuration);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        };
        context.Response.Cookies.Append("some-cookies", jwtToken, cookieOptions);
        return Results.Ok();
    }
}