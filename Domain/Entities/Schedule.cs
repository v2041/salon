using Domain.Enums;
using Domain.Exceptions;
using Domain.Services;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Schedule
{
    private Schedule()
    {
    }

    private Schedule(
        Guid id,
        bool isWorking,
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

    public IReadOnlyCollection<Appointment> Appointments
        => _appointments;

    private readonly List<Appointment> _appointments = [];

    public static Schedule Create(
        DateOnly date,
        bool isWorking,
        TimeInterval workInterval,
        TimeInterval? breakInterval
    )
    {
        if (date < DateOnly.FromDateTime(DateTime.Now) ||
            (date == DateOnly.FromDateTime(DateTime.Now) &&
             workInterval.Start < TimeOnly.FromDateTime(DateTime.Now)))
        {
            throw new BusinessException("Нельзя назначить рабочий день в прошлом.");
        }

        if (breakInterval != null && !breakInterval.IsInside(workInterval))
            throw new BusinessException("Заданное время перерыва не входит в рабочее время.");
        if (breakInterval?.Start == workInterval.Start || breakInterval?.End == workInterval.End)
            throw new BusinessException("Время перерыва не должно граничить с временем начала или конца работы.");
        if (workInterval.Duration < TimeSpan.FromHours(2))
            throw new BusinessException("Время работы должно быть не меньше двух часов.");
        return new Schedule(
            Guid.NewGuid(),
            isWorking,
            date,
            workInterval,
            breakInterval
        );
    }

    public Appointment AddAppointment(Guid userId, TimeOnly startTime, List<AppointmentOfferingData> appointmentOfferingData)
    {
        var interval = TimelineBuilder.BuildInterval(startTime, appointmentOfferingData);
        if (DateTime.Now > Date.ToDateTime(interval.Start))
            throw new BusinessException("Нельзя создать запись в прошлом.");
        if (!interval.IsInside(WorkInterval))
            throw new BusinessException("Не рабочее время недоступно для записи.");
        if (BreakInterval != null && interval.IsOverlapping(BreakInterval))
            throw new BusinessException("Запись не должна занимать время перерыва.");
        if (_appointments.Any(a =>
                a.Interval.IsOverlapping(interval) &&
                a.Status is AppointmentStatus.Confirmed or AppointmentStatus.Pending))
            throw new BusinessException("Запись не должна занимать время других записей.");
        var appointment = Appointment.Create(this, userId, appointmentOfferingData, interval);
        _appointments.Add(appointment);
        return appointment;
    }

    public void ChangeBreakInterval(TimeInterval? newBreakInterval)
    {
        if (newBreakInterval is not null && !newBreakInterval.IsInside(WorkInterval))
            throw new BusinessException("Заданное время перерыва не входит в рабочее время.");
        if (newBreakInterval?.Start == WorkInterval.Start || newBreakInterval?.End == WorkInterval.End)
            throw new BusinessException("Время перерыва не должно граничить с временем начала или конца работы.");
        if (newBreakInterval is not null && _appointments.Any(a =>
                a.Interval.IsOverlapping(newBreakInterval) &&
                a.Status is AppointmentStatus.Confirmed or AppointmentStatus.Pending))
            throw new BusinessException("Заданное время перерыва пересекается с назначенными записями.");
        BreakInterval = newBreakInterval;
    }

    public void ChangeWorkInterval(TimeInterval newWorkInterval)
    {
        if (BreakInterval is not null && !BreakInterval.IsInside(newWorkInterval))
            throw new BusinessException("Заданное время перерыва не входит в рабочее время.");
        if (_appointments.Any(a =>
                !a.Interval.IsInside(newWorkInterval) &&
                a.Status is AppointmentStatus.Confirmed or AppointmentStatus.Pending))
            throw new BusinessException("Назначенные записи не входят в заданное рабочее время.");
        WorkInterval = newWorkInterval;
    }

    public void ChangeIsWorking(bool isWorking)
    {
        if (_appointments.Any(a => a.Status is AppointmentStatus.Pending or AppointmentStatus.Confirmed))
            throw new BusinessException("Нельзя завершить рабочий день с назначенными записями.");
        IsWorking = isWorking;
    }
}