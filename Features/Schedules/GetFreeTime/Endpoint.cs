using Domain.Services;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.GetFreeTime;

public static class Endpoint
{
    public static async Task<IResult> GetFreeTimeAsync(
        [AsParameters] GetFreeTimeRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var schedule = await db.Schedules
            .Include(s => s.Appointments)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Date == request.Date, token);
        if (schedule == null) return Results.NotFound();

        var response = new GetFreeTimeResponse(TimelineBuilder.FindFreeIntervals(schedule).ToList());
        return Results.Ok(response);
    }
}