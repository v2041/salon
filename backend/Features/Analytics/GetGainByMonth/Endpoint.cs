using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Analytics.GetGainByMonth;

public static class Endpoint
{
    public static async Task<IResult> GetGainByMonthAsync(
        [AsParameters] GetGainByMonthRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var rows = await db.Appointments
            .AsNoTracking()
            .Where(a => a.Status == AppointmentStatus.Completed &&
                        a.Schedule.Date >= request.From && a.Schedule.Date <= request.To)
            .Select(a => new
            {
                a.Schedule.Date,
                Amount = a.Offerings.Sum(o => o.Price.Value)
            })
            .GroupBy(x => new { x.Date.Year, x.Date.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Gain = g.Sum(x => x.Amount)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(token);

        var response = new GetGainByMonthResponse(rows
            .Select(r => new MonthlyGain($"{r.Year:D4}-{r.Month:D2}", Money.FromDecimal(r.Gain)))
            .ToList());
        return Results.Ok(response);
    }
}
