using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Schedule
{
    private Schedule()
    {
    }

    private Schedule(
        Guid id,
        bool  isWorking,
        DateOnly date,
        TimeInterval workInterval,
        TimeInterval? breakInterval
    )
    {
        Id = id;
        IsWorking = isWorking;
        Date = date;
        WorkInterval = workInterval;
        BreakInterval = breakInterval;
    }

    public Guid Id { get; }
    public bool IsWorking { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeInterval WorkInterval { get; private set; }
    public TimeInterval? BreakInterval { get; private set; }

    public static Schedule Create(
        DateOnly date,
        bool isWorking,
        TimeInterval workInterval,
        TimeInterval? breakInterval
    )
    {
        if (date < DateOnly.FromDateTime(DateTime.UtcNow) ||
            (date == DateOnly.FromDateTime(DateTime.UtcNow) &&
             workInterval.Start < TimeOnly.FromDateTime(DateTime.UtcNow)))
        {
            throw new BusinessException("Нельзя назначить рабочий день в прошлом");
        }

        if (breakInterval != null && !breakInterval.IsInside(workInterval))
            throw new BusinessException("Заданное время перерыва не входит в рабочее время");
        if (workInterval.Duration() < TimeSpan.FromHours(1))
            throw new BusinessException("Время работы должно быть не меньше одного часа");
        return new Schedule(
            Guid.NewGuid(),
            isWorking,
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