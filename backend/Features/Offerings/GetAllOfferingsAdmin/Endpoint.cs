using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Offerings.GetAllOfferingsAdmin;

public static class Endpoint
{
    public static async Task<IResult> GetAllOfferingsAdminAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var response = await db.Offerings
            .AsNoTracking()
            .OrderBy(o => o.Title)
            .Select(o => new GetAllOfferingsAdminResponse(
                o.Id,
                o.Title,
                o.Description,
                o.Price,
                o.Duration,
                o.Category,
                o.IsActive
            ))
            .ToListAsync(token);
        return Results.Ok(response);
    }
}
