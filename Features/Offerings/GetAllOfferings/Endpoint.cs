using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Offerings.GetAllOfferings;

public class Endpoint
{
    public static async Task<IResult> GetAllOfferingsAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var response = await db.Offerings
            .Where(o => o.IsActive)
            .Select(o => new GetAllOfferingsResponse(
                o.Id,
                o.Title,
                o.Description,
                o.Price,
                o.Duration
            ))
            .ToListAsync(token);
        return Results.Ok(response);
    }
}