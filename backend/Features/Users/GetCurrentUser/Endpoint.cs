using System.Security.Claims;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Users.GetCurrentUser;

public static class Endpoint
{
    public static async Task<IResult> GetCurrentUserAsync(
        HttpContext context,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var value = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(value, out var userId)) return Results.Unauthorized();

        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, token);
        if (user is null) return Results.Unauthorized();

        return Results.Ok(new GetCurrentUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Phone,
            user.Role
        ));
    }
}
