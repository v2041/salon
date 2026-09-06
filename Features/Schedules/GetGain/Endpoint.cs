using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.GetGain;

public static class Endpoint
{
    public static async Task<IResult> GetGainAsync(
        [AsParameters] GetGainRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var response = new GetGainResponse(Money.FromDecimal(
            await db.Schedules
                .AsNoTracking()
                .Where(s => s.Date >= request.From && s.Date <= request.To)
                .SelectMany(s => s.Appointments)
                .Where(a => a.Status == AppointmentStatus.Completed)
                .SelectMany(a => a.Offerings)
                .SumAsync(ao => ao.Price.Value, token)
        ));
        return Results.Ok(response);
    }
}