using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.ChangeSchedule;

public static class Endpoint
{
    public static async Task<IResult> ChangeScheduleAsync(
        ChangeScheduleRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var schedule = await db.Schedules
            .Include(s => s.Appointments)
            .Where(s => s.Id == request.Id)
            .FirstOrDefaultAsync(token);

        if (schedule == null) return Results.NotFound();

        schedule.ChangeWorkInterval(request.WorkInterval);
        schedule.ChangeBreakInterval(request.BreakInterval);
        schedule.ChangeIsWorking(request.IsWorking);

        await db.SaveChangesAsync(token);

        return Results.NoContent();
    }
}