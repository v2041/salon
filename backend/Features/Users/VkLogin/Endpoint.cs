using Domain.Entities;
using Domain.Exceptions;
using Domain.Services;
using Domain.ValueObjects;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Users.VkLogin;

public static class Endpoint
{
    public static async Task<IResult> VkLoginAsync(
        [FromBody] VkLoginRequest request,
        VkIdClient vkIdClient,
        SalonDbContext db,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        HttpContext context,
        CancellationToken token
    )
    {
        if (!request.Consent)
            return Results.BadRequest("Отметьте согласие на обработку персональных данных");

        VkUserInfo vkUser;
        try
        {
            vkUser = await vkIdClient.ExchangeCodeAsync(
                request.Code, request.DeviceId, request.CodeVerifier, request.RedirectUrl, token);
        }
        catch (VkIdException exception)
        {
            loggerFactory.CreateLogger(typeof(Endpoint)).LogWarning(exception, "Ошибка обмена кода VK ID");
            return Results.BadRequest("Не удалось войти через VK ID. Попробуйте ещё раз");
        }

        if (string.IsNullOrWhiteSpace(vkUser.Phone))
            return Results.BadRequest("VK не передал номер телефона. Разрешите доступ к номеру и попробуйте снова");

        PhoneNumber phone;
        try
        {
            phone = PhoneNumber.Create(vkUser.Phone);
        }
        catch (BusinessException)
        {
            return Results.BadRequest("Вход доступен только с российским номером телефона");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.VkId == vkUser.Id, token);
        var isNew = false;
        if (user is null)
        {
            user = await db.Users.FirstOrDefaultAsync(u => u.Phone == phone, token);
            if (user is null)
            {
                user = User.Create(
                    string.IsNullOrWhiteSpace(vkUser.FirstName) ? User.DEFAULT_FIRST_NAME : vkUser.FirstName.Trim(),
                    string.IsNullOrWhiteSpace(vkUser.LastName) ? null : vkUser.LastName.Trim(),
                    phone,
                    DateTime.UtcNow);
                db.Users.Add(user);
                isNew = true;
            }
        }

        user.LinkVkId(vkUser.Id);
        AdminPolicy.PromoteIfAdminPhone(user, configuration["Admin:Phone"]);
        user.MarkLoggedIn();
        user.RecordConsent(ConsentPolicy.CurrentVersion, DateTime.UtcNow);
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
        return Results.Ok(new VkLoginResponse(isNew));
    }
}
