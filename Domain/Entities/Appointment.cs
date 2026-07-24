using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Appointment
{
    private Appointment()
    {
    }

    private Appointment(
        Guid id,
        Guid userId,
        DateOnly date,
        TimeInterval interval,
        decimal price,
        AppointmentStatus status
    )
    {
        Id = id;
        UserId = userId;
        Date = date;
        Interval = interval;
        Price = price;
        Status = status;
    }

    public Guid Id { get; }
    public Guid UserId { get; }
    public DateOnly Date { get;  private set;}
    public TimeInterval Interval { get; private set; }
    public decimal Price { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public User User { get; }
    private readonly List<Offering> _offerings = [];
    public IReadOnlyCollection<Offering> Offerings => _offerings;

    public static Appointment Create(
        Guid userId,
        DateOnly date,
        TimeInterval interval,
        decimal price,
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
            price,
            AppointmentStatus.Pending
        );
    }
    public void AddOffering(Offering offering)
    {
        _offerings.Add(offering); // ✅ Работает через приватную коллекцию
    }
    // public void AddOffering(
    //     Offering offering,
    //     TimeInterval workInterval,
    //     IEnumerable<TimeInterval> busyIntervals)
    // {
    //     if (Status != AppointmentStatus.Pending && Status != AppointmentStatus.Confirmed)
    //         throw new BusinessException("Действие недоступно для текущего статуса записи");
    //     if (_offerings.Any(o => o.Id == offering.Id))
    //         throw new BusinessException("Услуга уже добавлена");
    //     var newInterval = TimeInterval.Create(Interval.Start, Interval.End.Add(offering.Duration));
    //     if (!newInterval.IsInside(workInterval) || busyIntervals.Any(b => b.IsOverlapping(newInterval)))
    //         throw new BusinessException("Время недоступно");
    //
    //     _offerings.Add(offering);
    //     Interval = newInterval;
    // }
    //
    // public void RemoveOffering(Guid offeringId, TimeSpan duration)
    // {
    //     if (Status != AppointmentStatus.Pending && Status != AppointmentStatus.Confirmed)
    //         throw new BusinessException("Действие недоступно для текущего статуса записи");
    //
    //     if (_offerings.Count == 1)
    //         throw new BusinessException("Нельзя удалить последнюю услугу");
    //
    //     var appointmentOffering = _offerings.FirstOrDefault(o => o.Id == offeringId);
    //
    //     if (appointmentOffering != null)
    //     {
    //         _offerings.Remove(appointmentOffering);
    //         Interval = TimeInterval.Create(Interval.Start, Interval.End.Add(-duration));
    //     }
    //     else
    //         throw new BusinessException("Услуга не найдена");
    // }
    public void ChangePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new BusinessException("Цена должна быть больше 0");
        }
        Price = price;
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