using Domain.Services;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Analytics.GetWorkLoadRate;

public static class Endpoint
{
    public static async Task<IResult> GetWorkLoadRateAsync(
        [AsParameters] GetWorkLoadRateRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var schedules = await db.Schedules
            .AsNoTracking()
            .Where(s => s.Date >= request.From && s.Date <= request.To && s.IsWorking)
            .Include(s => s.Appointments)
            .ToListAsync(token);

        var workDuration = TimeInterval.IntervalsDuration(schedules.Select(s => s.WorkInterval));
        var busyDuration = TimeInterval.IntervalsDuration(schedules.SelectMany(TimelineBuilder.FindBusyIntervals));

        var response = new GetWorkLoadRateResponse(
            workDuration > TimeSpan.Zero
                ? busyDuration.TotalMinutes / workDuration.TotalMinutes
                : 0);
        return Results.Ok(response);
    }
}