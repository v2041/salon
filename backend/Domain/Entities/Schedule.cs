using Domain.Enums;
using Domain.Exceptions;
using Domain.Services;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Schedule
{
    private static readonly TimeSpan MinimumWorkDuration = TimeSpan.FromHours(2);

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
    public TimeInterval WorkInterval { get; private set; } = null!;
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
        if (workInterval.Duration < MinimumWorkDuration)
            throw new BusinessException("Время работы должно быть не меньше двух часов.");
        return new Schedule(
            Guid.NewGuid(),
            isWorking,
            date,
            workInterval,
            breakInterval
        );
    }

    public void CheckAppointmentInterval(TimeInterval interval)
    {
        EnsureAppointmentInterval(interval);
    }

    internal void EnsureAppointmentInterval(TimeInterval interval, Guid? exceptAppointmentId = null, bool checkPast = true)
    {
        if (checkPast && DateTime.Now > Date.ToDateTime(interval.Start))
            throw new BusinessException("Нельзя создать запись в прошлом.");
        if (!interval.IsInside(WorkInterval))
            throw new BusinessException("Не рабочее время недоступно для записи.");
        if (BreakInterval != null && interval.IsOverlapping(BreakInterval))
            throw new BusinessException("Запись не должна занимать время перерыва.");
        if (_appointments.Any(a =>
                a.Id != exceptAppointmentId &&
                a.Interval.IsOverlapping(interval) &&
                a.Status is AppointmentStatus.Confirmed or AppointmentStatus.Pending))
            throw new BusinessException("Запись не должна занимать время других записей.");
    }

    public Appointment AddAppointment(Guid userId, TimeOnly startTime,
        List<AppointmentOfferingData> appointmentOfferingData)
    {
        if (!IsWorking)
            throw new BusinessException("Нельзя записаться на выходной день.");
        var interval = TimelineBuilder.BuildInterval(startTime, appointmentOfferingData);
        CheckAppointmentInterval(interval);
        var appointment = Appointment.Create(this, userId, appointmentOfferingData, interval);
        _appointments.Add(appointment);
        return appointment;
    }

    public void AttachAppointment(Appointment appointment)
    {
        if (!IsWorking)
            throw new BusinessException("Нельзя записаться на выходной день.");
        CheckAppointmentInterval(appointment.Interval);
        _appointments.Add(appointment);
        appointment.ChangeSchedule(this);
    }

    public Appointment DetachAppointment(Guid appointmentId)
    {
        var appointment = Appointments.FirstOrDefault(a => a.Id == appointmentId);
        if (appointment is null)
            throw new BusinessException("Запись не найдена");

        _appointments.Remove(appointment);
        return appointment;
    }

    public void MoveAppointment(Guid appointmentId, TimeOnly startTime)
    {
        var appointment = _appointments.FirstOrDefault(a => a.Id == appointmentId);
        if (appointment is null)
            throw new BusinessException("Запись не найдена");
        var interval = TimeInterval.Create(startTime, startTime.Add(appointment.Interval.Duration));
        EnsureAppointmentInterval(interval, appointment.Id,
            checkPast: appointment.Status is AppointmentStatus.Pending or AppointmentStatus.Confirmed);
        appointment.ChangeInterval(interval);
    }

    public void RemoveAppointment(Guid appointmentId)
    {
        var appointment = _appointments.FirstOrDefault(a => a.Id == appointmentId);
        if (appointment is null)
            throw new BusinessException("Запись не найдена");
        _appointments.Remove(appointment);
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
        if (newWorkInterval.Duration < MinimumWorkDuration)
            throw new BusinessException("Время работы должно быть не меньше двух часов.");
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
        if (!isWorking && _appointments.Any(a => a.Status is AppointmentStatus.Pending or AppointmentStatus.Confirmed))
            throw new BusinessException("Нельзя завершить рабочий день с назначенными записями.");
        IsWorking = isWorking;
    }
}
