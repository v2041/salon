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
        if (!await db.VerificationCodes.AnyAsync(v => v.Phone == request.Phone && v.Code == request.Code, token))
            return Results.BadRequest("Неверный код");

        var user = User.Create(request.FirstName, request.LastName, request.Phone, DateTime.Now);
        return Results.Created($"/users/{user.Id}", user);
    }
}