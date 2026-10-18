using Domain.ValueObjects;

namespace Features.Schedules.GetAllSchedules;

public record GetScheduleResponse(
    Guid Id,
    DateOnly Date,
    bool IsWorking,
    TimeInterval WorkInterval,
    TimeInterval? BreakInterval
);