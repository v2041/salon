using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Users.Register;

public static class Endpoint
{
    public static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        if (await db.Users.AnyAsync(u => u.Phone == request.Phone, token))
            return Results.BadRequest("Номер телефона уже зарегистрирован");
        var code = await db.VerificationCodes.FirstOrDefaultAsync(v =>
            v.Phone == request.Phone && v.Code == request.Code && v.ExpiresAt > DateTime.UtcNow, token);
        if (code is null)
            return Results.BadRequest("Неверный код");
        var user = User.Create(request.FirstName, request.LastName, request.Phone, DateTime.UtcNow);
        db.VerificationCodes.Remove(code);
        db.Users.Add(user);
        await db.SaveChangesAsync(token);
        return Results.Created($"/users/{user.Id}", user);
    }
}