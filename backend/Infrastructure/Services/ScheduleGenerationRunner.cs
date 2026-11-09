using Domain.Entities;
using Domain.Services;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>
/// Общий сценарий работы с расписанием: загрузка будущих дней с записями,
/// построение плана, применение и сохранение.
/// </summary>
public class ScheduleGenerationRunner(SalonDbContext db)
{
    /// <summary>
    /// Фоновая генерация: отсутствующие дни от завтра до горизонта и выравнивание Auto-дней.
    /// Переключатель генерации учитывает план.
    /// </summary>
    public async Task GenerateAsync(SalonSettings settings, CancellationToken token)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var schedules = await LoadFutureDaysAsync(today, token, today.AddDays(settings.HorizonDays));
        var plan = SchedulePlanner.BuildGenerationPlan(
            settings,
            schedules.Select(ScheduleDayState.FromSchedule).ToList(),
            today);
        await RunAsync(plan, schedules, token);
    }

    /// <summary>
    /// Немедленное применение настроек: все будущие дни, независимо от переключателя.
    /// Возвращает число созданных и изменённых дней.
    /// </summary>
    public async Task<int> ApplyAsync(SalonSettings settings, CancellationToken token)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var schedules = await LoadFutureDaysAsync(today, token);
        var plan = SchedulePlanner.BuildApplyPlan(
            settings,
            schedules.Select(ScheduleDayState.FromSchedule).ToList(),
            today);
        return await RunAsync(plan, schedules, token);
    }

    private async Task<List<Schedule>> LoadFutureDaysAsync(
        DateOnly today,
        CancellationToken token,
        DateOnly? horizonEnd = null)
    {
        var query = db.Schedules
            .Include(s => s.Appointments)
            .Where(s => s.Date > today);
        if (horizonEnd is not null)
        {
            var end = horizonEnd.Value;
            query = query.Where(s => s.Date <= end);
        }

        return await query.ToListAsync(token);
    }

    private async Task<int> RunAsync(
        IReadOnlyList<SchedulePlanItem> plan,
        IReadOnlyCollection<Schedule> schedules,
        CancellationToken token)
    {
        var created = SchedulePlanner.Apply(plan, schedules.ToDictionary(s => s.Date));
        db.Schedules.AddRange(created);
        await db.SaveChangesAsync(token);
        return plan.Count(item => item.Action is SchedulePlanAction.Create or SchedulePlanAction.Change);
    }
}
