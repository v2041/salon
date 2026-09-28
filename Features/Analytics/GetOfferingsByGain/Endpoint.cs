using Domain.Enums;
using Domain.ValueObjects;
using Features.Analytics.GetOfferingsByPopularity;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Analytics.GetOfferingsByGain;

public static class Endpoint
{
    public static async Task<IResult> GetOfferingsByGainAsync(
        [AsParameters] GetOfferingsByGainRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var rows = await db.Offerings
            .AsNoTracking()
            .GroupJoin(
                db.Appointments
                    .AsNoTracking()
                    .Where(a => a.Status == AppointmentStatus.Completed && a.Schedule.Date >= request.From &&
                                a.Schedule.Date <= request.To)
                    .SelectMany(a => a.Offerings),
                offering => offering.Id,
                appointmentOffering => appointmentOffering.OfferingId,
                (offering, appointmentOfferings) => new
                {
                    Name = offering.Title,
                    PriceValue = offering.Price.Value,
                    Count = appointmentOfferings.Count()
                })
            .Where(x => x.Count > 0)
            .OrderByDescending(x => x.PriceValue)
            .ThenBy(x => x.Name)
            .ToListAsync(token);

        var response = rows
            .Select(r => new GetOfferingsByGainResponse(
                r.Name,
                r.Count,
                Money.FromDecimal(r.PriceValue)))
            .ToList();

        return Results.Ok(response);
    }
}