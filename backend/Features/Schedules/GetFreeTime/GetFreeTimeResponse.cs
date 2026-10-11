using Domain.ValueObjects;

namespace Features.Schedules.GetFreeTime;

public record GetFreeTimeResponse(IEnumerable<TimeInterval> FreeIntervals);