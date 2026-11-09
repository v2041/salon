using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Services;

/// <summary>Действие плана расписания.</summary>
public enum SchedulePlanAction
{
    Create,
    Change,
    Skip
}

/// <summary>Причина, по которой планировщик не трогает день.</summary>
public enum SchedulePlanSkipReason
{
    None = 0,
    Manual,
    HasActiveAppointments,
    AlreadyMatches
}

/// <summary>Целевое состояние дня, собранное из настроек мастера.</summary>
public sealed record SchedulePlanTarget(
    bool IsWorking,
    TimeInterval WorkInterval,
    TimeInterval? BreakInterval
);

/// <summary>Снимок существующего дня для планировщика.</summary>
public sealed record ScheduleDayState(
    DateOnly Date,
    ScheduleSource Source,
    bool IsWorking,
    TimeInterval WorkInterval,
    TimeInterval? BreakInterval,
    bool HasActiveAppointments
)
{
    public static ScheduleDayState FromSchedule(Schedule schedule) =>
        new(
            schedule.Date,
            schedule.Source,
            schedule.IsWorking,
            schedule.WorkInterval,
            schedule.BreakInterval,
            schedule.HasActiveAppointments);
}

/// <summary>Пункт плана: создать, изменить или пропустить день.</summary>
public sealed record SchedulePlanItem(
    DateOnly Date,
    SchedulePlanAction Action,
    SchedulePlanTarget? Target,
    SchedulePlanSkipReason SkipReason
)
{
    public static SchedulePlanItem Create(DateOnly date, SchedulePlanTarget target) =>
        new(date, SchedulePlanAction.Create, target, SchedulePlanSkipReason.None);

    public static SchedulePlanItem Change(DateOnly date, SchedulePlanTarget target) =>
        new(date, SchedulePlanAction.Change, target, SchedulePlanSkipReason.None);

    public static SchedulePlanItem Skip(DateOnly date, SchedulePlanSkipReason reason) =>
        new(date, SchedulePlanAction.Skip, null, reason);
}
