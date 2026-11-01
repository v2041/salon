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
        var data = await db.Appointments
            .AsNoTracking()
            .Where(a => a.Schedule.Date >= request.From && a.Schedule.Date <= request.To)
            .GroupBy(a => 1)
            .Select(g => new
            {
                completedCount = g.Count(a => a.Status == AppointmentStatus.Completed),
                noShowCount = g.Count(a => a.Status == AppointmentStatus.Missed)
            })
            .FirstOrDefaultAsync(token);
        var rate = data is null || data.completedCount + data.noShowCount == 0
            ? 0
            : (double)data.noShowCount / (data.completedCount + data.noShowCount);
        var response = new GetNoShowRateResponse(rate);
        return Results.Ok(response);
    }
}