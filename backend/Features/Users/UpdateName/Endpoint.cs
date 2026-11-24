using System.Security.Claims;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Users.UpdateName;

public static class Endpoint
{
    public static async Task<IResult> UpdateNameAsync(
        [FromBody] UpdateNameRequest request,
        HttpContext context,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var value = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(value, out var userId)) return Results.Unauthorized();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, token);
        if (user is null) return Results.Unauthorized();

        user.UpdateName(request.FirstName, request.LastName);
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}
