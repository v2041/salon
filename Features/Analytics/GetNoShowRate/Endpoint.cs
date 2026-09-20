using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Analytics.GetNoShowRate;

public static class Endpoint
{
    public static async Task<IResult> GetNoShowRateAsync(
        [AsParameters] GetNoShowRateRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var data = await db.Schedules
            .Where(s => s.Date >= request.From && s.Date <= request.To)
            .SelectMany(s => s.Appointments)
            .GroupBy(a => 1)
            .Select(g => new
            {
                completedCount = g.Count(a => a.Status == AppointmentStatus.Completed),
                noShowCount = g.Count(a => a.Status == AppointmentStatus.Missed)
            })
            .FirstOrDefaultAsync(token);
        if (data == null) return Results.NotFound();
        var response = new GetNoShowRateResponse(data.completedCount + data.noShowCount > 0
            ? (double)data.noShowCount / (data.completedCount + data.noShowCount)
            : 0);
        return Results.Ok(response);
    }
}