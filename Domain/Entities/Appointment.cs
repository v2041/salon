using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Appointment
{
    private Appointment(Guid id, Guid userId, DateOnly date, TimeInterval interval, AppointmentStatus status)
    {
        Id = id;
        UserId = userId;
        Date = date;
        Interval = interval;
        Status = status;
    }

    public Guid Id { get; }
    public Guid UserId { get; }
    public DateOnly Date { get; }
    public TimeInterval Interval { get; private set; }
    public AppointmentStatus Status { get; private set; }

    private readonly List<AppointmentOffering> _appointmentOfferings = [];
    public IReadOnlyCollection<AppointmentOffering> AppointmentOfferings => _appointmentOfferings;

    public static Appointment Create(
        Guid userId,
        DateOnly date,
        TimeInterval interval,
        TimeInterval workInterval,
        IEnumerable<TimeInterval> busyIntervals
    )
    {
        if (!interval.IsInside(workInterval) || busyIntervals.Any(b => b.IsOverlapping(interval)))
            throw new BusinessException("Время недоступно");
        return new Appointment(
            Guid.NewGuid(),
            userId,
            date,
            interval,
            AppointmentStatus.Pending
        );
    }

    public void AddOffering(
        Guid offeringId,
        decimal price,
        TimeSpan duration,
        TimeInterval workInterval,
        IEnumerable<TimeInterval> busyIntervals)
    {
        if (Status != AppointmentStatus.Pending && Status != AppointmentStatus.Confirmed)
            throw new BusinessException("Действие недоступно для текущего статуса записи");
        if (_appointmentOfferings.Any(o => o.OfferingId == offeringId))
            throw new BusinessException("Услуга уже добавлена");
        var newInterval = TimeInterval.Create(Interval.Start, Interval.End.Add(duration));
        if (!newInterval.IsInside(workInterval) || busyIntervals.Any(b => b.IsOverlapping(newInterval)))
            throw new BusinessException("Время недоступно");

        _appointmentOfferings.Add(AppointmentOffering.Create(Id, offeringId, price));
        Interval = newInterval;
    }

    public void RemoveOffering(Guid offeringId, TimeSpan duration)
    {
        if (Status != AppointmentStatus.Pending && Status != AppointmentStatus.Confirmed)
            throw new BusinessException("Действие недоступно для текущего статуса записи");

        if (_appointmentOfferings.Count == 1)
            throw new BusinessException("Нельзя удалить последнюю услугу");

        var appointmentOffering = _appointmentOfferings.FirstOrDefault(o => o.OfferingId == offeringId);

        if (appointmentOffering != null)
        {
            _appointmentOfferings.Remove(appointmentOffering);
            Interval = TimeInterval.Create(Interval.Start, Interval.End.Add(-duration));
        }
        else
            throw new BusinessException("Услуга не найдена");
    }

    public void Cancel()
    {
        if (Status != AppointmentStatus.Pending && Status != AppointmentStatus.Confirmed)
            throw new BusinessException("Действие недоступно для текущего статуса записи");

        if (Status == AppointmentStatus.Confirmed &&
            (Date.ToDateTime(Interval.Start) - DateTime.UtcNow).TotalHours < 2)
            throw new BusinessException("Нельзя отменить запись менее чем за 2 часа до начала");

        Status = AppointmentStatus.Canceled;
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            throw new BusinessException("Действие недоступно для текущего статуса записи");
        Status = AppointmentStatus.Confirmed;
    }

    public void Reject()
    {
        if (Status != AppointmentStatus.Pending && Status != AppointmentStatus.Confirmed)
            throw new BusinessException("Действие недоступно для текущего статуса записи");
        Status = AppointmentStatus.Rejected;
    }

    public void Miss()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new BusinessException("Действие недоступно для текущего статуса записи");
        Status = AppointmentStatus.Missed;
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new BusinessException("Действие недоступно для текущего статуса записи");
        Status = AppointmentStatus.Completed;
    }
}