using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Services;

public static class TimelineBuilder
{
    public static TimeInterval BuildInterval(TimeOnly startTime, IEnumerable<Offering> offerings)
    {
        var duration = offerings.Aggregate(TimeSpan.Zero, (current, offering) => current + offering.Duration);
        return TimeInterval.Create(startTime, startTime.Add(duration));
    }
    public static TimeInterval BuildInterval(TimeOnly startTime, IEnumerable<AppointmentOfferingData> offerings)
    {
        var duration = offerings.Aggregate(TimeSpan.Zero, (current, offering) => current + offering.Duration);
        return TimeInterval.Create(startTime, startTime.Add(duration));
    }

    public static IEnumerable<TimeInterval> FindBusyIntervals(Schedule schedule)
    {
        return schedule.Appointments
            .Where(a => a.Status is AppointmentStatus.Pending or AppointmentStatus.Confirmed)
            .Select(a => a.Interval)
            .Concat(schedule.BreakInterval != null ? [schedule.BreakInterval] : [])
            .OrderBy(i => i.Start);
    }

    public static IEnumerable<TimeInterval> FindFreeIntervals(Schedule schedule)
    {
        if (!schedule.IsWorking)
            return [];
        var busyIntervals = FindBusyIntervals(schedule);
        var freeIntervals = new List<TimeInterval>();
        var currentStart = schedule.Date == DateOnly.FromDateTime(DateTime.Now)
            ? TimeOnly.FromDateTime(DateTime.Now)
            : schedule.WorkInterval.Start;

        foreach (var busy in busyIntervals)
        {
            if (currentStart < busy.Start)
                freeIntervals.Add(TimeInterval.Create(currentStart, busy.Start));

            if (busy.End > currentStart)
                currentStart = busy.End;
        }

        if (currentStart < schedule.WorkInterval.End)
            freeIntervals.Add(TimeInterval.Create(currentStart, schedule.WorkInterval.End));

        return freeIntervals;
    }
}