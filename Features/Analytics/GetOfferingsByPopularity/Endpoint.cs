using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Analytics.GetOfferingsByPopularity;

public static class Endpoint
{
    public static async Task<IResult> GetOfferingsByPopularityAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var rows = await db.Offerings
            .AsNoTracking()
            .GroupJoin(
                db.Appointments
                    .AsNoTracking()
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .SelectMany(a => a.Offerings),
                
                offering => offering.Id,
                
                appointmentOffering => appointmentOffering.OfferingId,
                
                (offering, appointmentOfferings) => new {
                    Name = offering.Title,
                    PriceValue = offering.Price.Value,
                    Count = appointmentOfferings.Count()
                })
            .Where(x => x.Count > 0)
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Name)
            .ToListAsync(token);

        var response = rows
            .Select(r => new GetOfferingsByPopularityResponse(
                r.Name,
                r.Count,
                Money.FromDecimal(r.PriceValue)))
            .ToList();

        return Results.Ok(response);
    }
}
