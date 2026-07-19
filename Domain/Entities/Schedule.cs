using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Schedule
{
    private Schedule(
        Guid id,
        DateOnly date,
        TimeInterval workInterval,
        TimeInterval? breakInterval
    )
    {
        Id = id;
        Date = date;
        WorkInterval = workInterval;
        BreakInterval = breakInterval;
    }

    public Guid Id { get; }
    public DateOnly Date { get; }
    public TimeInterval WorkInterval { get; private set; }
    public TimeInterval? BreakInterval { get; private set; }

    public static Schedule Create(
        DateOnly date,
        TimeInterval workInterval,
        TimeInterval? breakInterval
    )
    {
        return new Schedule(
            Guid.NewGuid(),
            date,
            workInterval,
            breakInterval
        );
    }

    public void ChangeBreakInterval(TimeInterval? newBreakInterval, IEnumerable<TimeInterval> appointmentIntervals)
    {
        if (newBreakInterval is not null && !newBreakInterval.IsInside(WorkInterval))
            throw new BusinessException("Заданное время перерыва не входит в рабочее время");
        if (newBreakInterval is not null && appointmentIntervals.Any(a => a.IsOverlapping(newBreakInterval)))
            throw new BusinessException("Заданное время перерыва пересекается с назначенными записями");
        BreakInterval = newBreakInterval;
    }

    public void ChangeWorkInterval(TimeInterval newWorkInterval, IEnumerable<TimeInterval> appointmentIntervals)
    {
        if (BreakInterval is not null && !BreakInterval.IsInside(newWorkInterval))
            throw new BusinessException("Заданное время перерыва не входит в рабочее время");
        if (appointmentIntervals.Any(a => !a.IsInside(newWorkInterval)))
            throw new BusinessException("Назначенные записи не входят в заданное рабочее время");
        WorkInterval = newWorkInterval;
    }
}