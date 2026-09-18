using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Offerings.GetAllOfferings;

public static class Endpoint
{
    public static async Task<IResult> GetAllOfferingsAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var response = await db.Offerings
            .AsNoTracking()
            .Where(o => o.IsActive)
            .Select(o => new GetAllOfferingsResponse(
                o.Id,
                o.Title,
                o.Description,
                o.Price,
                o.Duration,
                o.Category
            ))
            .ToListAsync(token);
        return Results.Ok(response);
    }
}
