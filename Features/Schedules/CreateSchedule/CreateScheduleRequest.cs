using Domain.ValueObjects;

namespace Features.Schedules.CreateSchedule;

public record CreateScheduleRequest(DateOnly Date, TimeInterval WorkInterval, TimeInterval? BreakInterval);