using Infrastructure.Authentication;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Users.Login;

public static class Endpoint
{
    public static async Task<IResult> LoginAsync(
        LoginRequest request,
        SalonDbContext db,
        IConfiguration configuration,
        HttpContext context,
        CancellationToken token
    )
    {
        var code = await db.VerificationCodes.FirstOrDefaultAsync(x =>
            x.Phone == request.Phone && x.Code == request.Code && x.ExpiresAt > DateTime.UtcNow, token);
        if (code == null)
            return Results.BadRequest("Неверный номер телефона или код");
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