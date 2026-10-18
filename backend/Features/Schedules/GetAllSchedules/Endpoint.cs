using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.GetAllSchedules;

public static class Endpoint
{
    public static async Task<IResult> GetAllSchedulesAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var schedules = await db.Schedules
            .Where(s => s.Date >= DateOnly.FromDateTime(DateTime.Now))
            .OrderBy(s => s.Date)
            .ToListAsync(token);

        var responses = schedules.Select(s =>
            new GetScheduleResponse(
                s.Id,
                s.Date,
                s.IsWorking,
                s.WorkInterval,
                s.BreakInterval
            )
        );
        return Results.Ok(responses);
    }
}