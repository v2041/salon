using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Services;

public static class ScheduleRules
{
    private static readonly TimeSpan MinimumWorkDuration = TimeSpan.FromHours(2);

    public static void EnsureWorkIntervalDuration(TimeInterval workInterval)
    {
        if (workInterval.Duration < MinimumWorkDuration)
            throw new BusinessException("Время работы должно быть не меньше двух часов.");
    }

    public static void EnsureBreakInsideWorkInterval(TimeInterval workInterval, TimeInterval? breakInterval)
    {
        if (breakInterval is not null && !breakInterval.IsInside(workInterval))
            throw new BusinessException("Заданное время перерыва не входит в рабочее время.");
    }

    public static void EnsureBreakDoesNotTouchBorders(TimeInterval workInterval, TimeInterval? breakInterval)
    {
        if (breakInterval?.Start == workInterval.Start || breakInterval?.End == workInterval.End)
            throw new BusinessException("Время перерыва не должно граничить с временем начала или конца работы.");
    }
}
