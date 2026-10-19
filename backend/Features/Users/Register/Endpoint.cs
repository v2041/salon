using Domain.Entities;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Users.Register;

public static class Endpoint
{
    public static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        SalonDbContext db,
        IConfiguration configuration,
        HttpContext context,
        CancellationToken token
    )
    {
        if (await db.Users.AnyAsync(u => u.Phone == request.Phone, token))
            return Results.BadRequest("Номер телефона уже зарегистрирован");
        var code = await db.VerificationCodes.FirstOrDefaultAsync(v =>
            v.Phone == request.Phone, token);
        if (code is null)
            return Results.BadRequest("Неверный код");
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
                _ => "Неверный код"
            });
        }
        var user = User.Create(request.FirstName, request.LastName, request.Phone, DateTime.UtcNow);
        db.VerificationCodes.Remove(code);
        db.Users.Add(user);
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