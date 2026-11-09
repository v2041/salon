using Domain.Entities;
using Domain.Enums;

namespace Domain.Services;

/// <summary>
/// Планировщик расписания. Сравнивает настройки мастера с существующими днями
/// и возвращает план действий. Не меняет сущности и не работает с БД.
/// </summary>
public static class SchedulePlanner
{
    /// <summary>
    /// План фоновой генерации: от завтра до горизонта включительно.
    /// Выключенная генерация — пустой план.
    /// </summary>
    public static IReadOnlyList<SchedulePlanItem> BuildGenerationPlan(
        SalonSettings settings,
        IReadOnlyCollection<ScheduleDayState> existingDays,
        DateOnly today)
    {
        if (!settings.GenerationEnabled)
            return [];

        var plan = new List<SchedulePlanItem>();
        var byDate = existingDays.ToDictionary(day => day.Date);
        for (var offset = 1; offset <= settings.HorizonDays; offset++)
        {
            var date = today.AddDays(offset);
            var target = BuildTarget(settings, date);
            if (!byDate.TryGetValue(date, out var existing))
            {
                plan.Add(SchedulePlanItem.Create(date, target));
                continue;
            }

            if (existing.Source == ScheduleSource.Manual)
            {
                plan.Add(SchedulePlanItem.Skip(date, SchedulePlanSkipReason.Manual));
                continue;
            }

            if (existing.HasActiveAppointments)
            {
                plan.Add(SchedulePlanItem.Skip(date, SchedulePlanSkipReason.HasActiveAppointments));
                continue;
            }

            plan.Add(Matches(existing, target)
                ? SchedulePlanItem.Skip(date, SchedulePlanSkipReason.AlreadyMatches)
                : SchedulePlanItem.Change(date, target));
        }

        return plan;
    }

    /// <summary>
    /// План немедленного применения настроек: создаёт отсутствующие дни до горизонта
    /// и выравнивает все будущие дни без активных записей, включая дни за горизонтом.
    /// Работает независимо от переключателя генерации.
    /// </summary>
    public static IReadOnlyList<SchedulePlanItem> BuildApplyPlan(
        SalonSettings settings,
        IReadOnlyCollection<ScheduleDayState> existingDays,
        DateOnly today)
    {
        var plan = new List<SchedulePlanItem>();
        var byDate = existingDays.ToDictionary(day => day.Date);
        for (var offset = 1; offset <= settings.HorizonDays; offset++)
        {
            var date = today.AddDays(offset);
            if (!byDate.ContainsKey(date))
                plan.Add(SchedulePlanItem.Create(date, BuildTarget(settings, date)));
        }

        foreach (var existing in existingDays.Where(day => day.Date > today))
        {
            var target = BuildTarget(settings, existing.Date);
            if (existing.HasActiveAppointments)
                plan.Add(SchedulePlanItem.Skip(existing.Date, SchedulePlanSkipReason.HasActiveAppointments));
            else if (existing.Source == ScheduleSource.Auto && Matches(existing, target))
                plan.Add(SchedulePlanItem.Skip(existing.Date, SchedulePlanSkipReason.AlreadyMatches));
            else
                plan.Add(SchedulePlanItem.Change(existing.Date, target));
        }

        return plan.OrderBy(item => item.Date).ToList();
    }

    /// <summary>
    /// Применяет план через доменные методы. Возвращает созданные дни:
    /// вызывающий добавляет их в БД и сохраняет изменения.
    /// </summary>
    public static IReadOnlyList<Schedule> Apply(
        IEnumerable<SchedulePlanItem> plan,
        IReadOnlyDictionary<DateOnly, Schedule> schedulesByDate)
    {
        var created = new List<Schedule>();
        foreach (var item in plan)
        {
            if (item.Action == SchedulePlanAction.Skip)
                continue;

            var target = item.Target!;
            if (item.Action == SchedulePlanAction.Create)
                created.Add(Schedule.Create(
                    item.Date,
                    target.IsWorking,
                    target.WorkInterval,
                    target.BreakInterval,
                    source: ScheduleSource.Auto));
            else
                schedulesByDate[item.Date].AlignToSettings(
                    target.IsWorking,
                    target.WorkInterval,
                    target.BreakInterval);
        }

        return created;
    }

    private static SchedulePlanTarget BuildTarget(SalonSettings settings, DateOnly date) =>
        new(
            !settings.WeekendDays.Contains(date.DayOfWeek),
            settings.WorkInterval,
            settings.BreakInterval);

    private static bool Matches(ScheduleDayState existing, SchedulePlanTarget target) =>
        existing.IsWorking == target.IsWorking &&
        existing.WorkInterval == target.WorkInterval &&
        existing.BreakInterval == target.BreakInterval;
}
