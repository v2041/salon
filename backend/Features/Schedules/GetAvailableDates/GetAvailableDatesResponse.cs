namespace Features.Schedules.GetAvailableDates;

public record GetAvailableDatesResponse(IEnumerable<DateOnly> Dates);