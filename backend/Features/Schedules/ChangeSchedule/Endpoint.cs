using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.ChangeSchedule;

public static class Endpoint
{
    public static async Task<IResult> ChangeScheduleAsync(
        [FromBody] ChangeScheduleRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var schedule = await db.Schedules
            .Include(s => s.Appointments)
            .Where(s => s.Id == request.Id)
            .FirstOrDefaultAsync(token);

        if (schedule == null)
            throw new NotFoundException("Рабочий день не найден");

        schedule.ChangeWorkInterval(request.WorkInterval);
        schedule.ChangeBreakInterval(request.BreakInterval);
        schedule.ChangeIsWorking(request.IsWorking);

        await db.SaveChangesAsync(token);

        return Results.NoContent();
    }
}