using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.GetAllSchedules;

public class Endpoint
{
    public static async Task<IResult> GetAllSchedulesAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var schedules = await db.Schedules
            .Where(s => s.Date >= DateOnly.FromDateTime(DateTime.UtcNow))
            .ToListAsync(token);

        var responses = schedules.Select(s =>
            new GetScheduleResponse(
                s.Id,
                s.Date,
                s.WorkInterval,
                s.BreakInterval
            )
        );
        return Results.Ok(responses);
    }
}