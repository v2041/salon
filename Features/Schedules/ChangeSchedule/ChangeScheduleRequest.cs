using Domain.ValueObjects;

namespace Features.Schedules.ChangeSchedule;

public record ChangeScheduleRequest(
    Guid Id,
    bool IsWorking,
    TimeInterval WorkInterval,
    TimeInterval? BreakInterval
    );