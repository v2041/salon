using Domain.ValueObjects;

namespace Features.Schedules.GetAllSchedules;

public record GetScheduleResponse(Guid Id, DateOnly Date, TimeInterval WorkInterval, TimeInterval? BreakInterval);