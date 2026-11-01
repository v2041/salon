using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Analytics.GetLosses;

public static class Endpoint
{
    public static async Task<IResult> GetLossesAsync(
        [AsParameters] GetLossesRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var counts = await db.Appointments
            .AsNoTracking()
            .Where(a => a.Schedule.Date >= request.From && a.Schedule.Date <= request.To)
            .GroupBy(a => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Canceled = g.Count(a => a.Status == AppointmentStatus.Canceled),
                Rejected = g.Count(a => a.Status == AppointmentStatus.Rejected),
                Missed = g.Count(a => a.Status == AppointmentStatus.Missed)
            })
            .FirstOrDefaultAsync(token);

        var total = counts?.Total ?? 0;
        var canceled = counts?.Canceled ?? 0;
        var rejected = counts?.Rejected ?? 0;
        var missed = counts?.Missed ?? 0;

        var response = new GetLossesResponse(
            canceled,
            Rate(canceled, total),
            rejected,
            Rate(rejected, total),
            missed,
            Rate(missed, total)
        );
        return Results.Ok(response);
    }

    private static double Rate(int count, int total) => total > 0 ? (double)count / total : 0;
}
