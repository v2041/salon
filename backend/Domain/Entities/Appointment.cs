using Domain.Enums;
using Domain.Exceptions;
using Domain.Services;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Appointment
{
    private Appointment()
    {
    }

    private Appointment(
        Guid id,
        Schedule schedule,
        Guid userId,
        IEnumerable<AppointmentOffering> offerings,
        TimeInterval interval,
        AppointmentStatus status
    )
    {
        Id = id;
        Schedule = schedule;
        ScheduleId = schedule.Id;
        UserId = userId;
        _offerings = offerings.ToList();
        Interval = interval;
        Status = status;
    }

    public Guid Id { get; }
    public Schedule Schedule { get; private set; } = null!;
    public Guid ScheduleId { get; private set; }
    public Guid UserId { get; private set; }
    public TimeInterval Interval { get; private set; } = null!;
    public AppointmentStatus Status { get; private set; }
    public Money Price => Money.FromDecimal(_offerings.Sum(o => o.Price.Value));

    private readonly List<AppointmentOffering> _offerings = [];
    public IReadOnlyCollection<AppointmentOffering> Offerings => _offerings;

    public static Appointment Create(
        Schedule schedule,
        Guid userId,
        List<AppointmentOfferingData> appointmentOfferingData,
        TimeInterval interval
    )
    {
        if (appointmentOfferingData.Count == 0)
            throw new BusinessException("Услуги не выбраны");
        var id = Guid.NewGuid();
        var offerings = appointmentOfferingData
            .Select(d => new AppointmentOffering(id, d.OfferingId, d.Duration, d.Price));
        return new Appointment(
            id,
            schedule,
            userId,
            offerings,
            interval,
            AppointmentStatus.Pending
        );
    }

    internal void ChangeInterval(TimeInterval interval)
    {
        Interval = interval;
    }

    public void ChangeOfferingPrice(Guid offeringId, Money price)
    {
        var offering = _offerings.FirstOrDefault(o => o.OfferingId == offeringId);
        if (offering == null)
            throw new BusinessException("Услуга не найдена");
        offering.ChangePrice(price);
    }

    public void ChangeSchedule(Schedule schedule)
    {
        Schedule = schedule;
    }

    public void ChangeOfferingDuration(Guid offeringId, TimeSpan duration)
    {
        var offering = _offerings.FirstOrDefault(o => o.OfferingId == offeringId);
        if (offering == null)
            throw new BusinessException("Услуга не найдена");
        if (duration <= TimeSpan.FromMinutes(Offering.MIN_DURATION_MINUTES))
            throw new BusinessException($"Минимальная длительность услуги - {Offering.MIN_DURATION_MINUTES} минут.");
        var totalDuration = _offerings.Aggregate(
            TimeSpan.Zero,
            (current, o) => current + (o.OfferingId == offeringId ? duration : o.Duration));
        var endTime = Interval.Start.Add(totalDuration);
        if (endTime <= Interval.Start)
            throw new BusinessException("Не рабочее время недоступно для записи.");
        var newInterval = TimeInterval.Create(Interval.Start, endTime);
        Schedule.EnsureAppointmentInterval(newInterval, Id, checkPast: false);
        offering.ChangeDuration(duration);
        Interval = newInterval;
    }

    public void Cancel()
    {
        if (Status != AppointmentStatus.Pending && Status != AppointmentStatus.Confirmed)
            throw new BusinessException("Действие недоступно для текущего статуса записи");

        if (Status == AppointmentStatus.Confirmed &&
            (Schedule.Date.ToDateTime(Interval.Start) - DateTime.Now).TotalHours < 2)
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
