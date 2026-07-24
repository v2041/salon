using Domain.ValueObjects;

namespace Features.Schedules.CreateSchedule;

public record CreateScheduleResponse(Guid Id, DateOnly Date, TimeInterval WorkInterval, TimeInterval? BreakInterval);