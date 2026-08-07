using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.CreateSchedule;

public static class Endpoint
{
    public static async Task<IResult> CreateScheduleAsync(
        [FromBody] CreateScheduleRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        if (await db.Schedules.AnyAsync(s => s.Date == request.Date, token))
            return Results.Conflict();
        var schedule = Schedule.Create(request.Date, true, request.WorkInterval, request.BreakInterval);
        db.Schedules.Add(schedule);
        await db.SaveChangesAsync(token);
        var response = new CreateScheduleResponse(
            schedule.Id,
            schedule.Date,
            schedule.WorkInterval,
            schedule.BreakInterval
        );
        return Results.Created($"api/schedules/{schedule.Id}", response);
    }
}