using Domain.Exceptions;
using Domain.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Settings.ApplySettings;

public static class Endpoint
{
    public static async Task<IResult> ApplySettingsAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var settings = await db.SalonSettings.FirstOrDefaultAsync(token);
        if (settings is null)
            throw new NotFoundException("Настройки не найдены.");

        var today = DateOnly.FromDateTime(DateTime.Now);
        var schedules = await db.Schedules
            .Include(s => s.Appointments)
            .Where(s => s.Date > today)
            .ToListAsync(token);

        var plan = SchedulePlanner.BuildApplyPlan(
            settings,
            schedules.Select(ScheduleDayState.FromSchedule).ToList(),
            today);

        var created = SchedulePlanner.Apply(plan, schedules.ToDictionary(s => s.Date));
        db.Schedules.AddRange(created);
        await db.SaveChangesAsync(token);

        var affectedDays = plan.Count(item =>
            item.Action is SchedulePlanAction.Create or SchedulePlanAction.Change);
        return Results.Ok(new ApplySettingsResponse(affectedDays));
    }
}
