using System.Collections.Immutable;
using Domain.Enums;
using Domain.ValueObjects;
using Features.Analytics.AverageHourGain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Analytics.GetAverageHourGain;

public static class Endpoint
{
    public static async Task<IResult> GetAverageHourGainAsync(
        [AsParameters] GetAverageHourGainRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var gain = await db.Appointments
            .AsNoTracking()
            .Where(a => a.Status == AppointmentStatus.Completed &&
                        a.Schedule.Date >= request.From && a.Schedule.Date <= request.To)
            .SelectMany(a => a.Offerings)
            .SumAsync(ao => (decimal?)ao.Price.Value, token) ?? 0m;

        var intervals = await db.Schedules
            .AsNoTracking()
            .Where(s => s.Date >= request.From && s.Date <= request.To && s.IsWorking)
            .Select(s => new { s.WorkInterval.Start, s.WorkInterval.End })
            .ToListAsync(token);

        var workHours = intervals.Sum(s => (s.End - s.Start).TotalHours);

        var response = new GetAverageHourGainResponse(Money.FromDecimal(
            workHours > 0 ? gain / (decimal)workHours : 0
        ));
        return Results.Ok(response);
    }
}