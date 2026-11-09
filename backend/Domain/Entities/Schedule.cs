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
        TimeInterval? breakInterval,
        ScheduleSource source
    )
    {
        Id = id;
        IsWorking = isWorking;
        Date = date;
        WorkInterval = workInterval;
        BreakInterval = breakInterval;
        Source = source;
    }

    public Guid Id { get; }
    public bool IsWorking { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeInterval WorkInterval { get; private set; } = null!;
    public TimeInterval? BreakInterval { get; private set; }
    public ScheduleSource Source { get; private set; } = ScheduleSource.Manual;

    public IReadOnlyCollection<Appointment> Appointments
        => _appointments;

    public bool HasActiveAppointments
        => _appointments.Any(IsActive);

    private static bool IsActive(Appointment appointment) =>
        appointment.Status is AppointmentStatus.Pending or AppointmentStatus.Confirmed;

    private readonly List<Appointment> _appointments = [];

    public static Schedule Create(
        DateOnly date,
        bool isWorking,
        TimeInterval workInterval,
        TimeInterval? breakInterval,
        bool checkPast = true,
        ScheduleSource source = ScheduleSource.Manual
    )
    {
        if (checkPast && (date < DateOnly.FromDateTime(DateTime.Now) ||
            (date == DateOnly.FromDateTime(DateTime.Now) &&
             workInterval.Start < TimeOnly.FromDateTime(DateTime.Now))))
        {
            throw new BusinessException("Нельзя назначить рабочий день в прошлом.");
        }

        ScheduleRules.EnsureBreakInsideWorkInterval(workInterval, breakInterval);
        ScheduleRules.EnsureBreakDoesNotTouchBorders(workInterval, breakInterval);
        ScheduleRules.EnsureWorkIntervalDuration(workInterval);
        return new Schedule(
            Guid.NewGuid(),
            isWorking,
            date,
            workInterval,
            breakInterval,
            source
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
                IsActive(a)))
            throw new BusinessException("Запись не должна занимать время других записей.");
    }

    public Appointment AddAppointment(Guid userId, TimeOnly startTime,
        List<AppointmentOfferingData> appointmentOfferingData, bool checkPast = true)
    {
        if (!IsWorking)
            throw new BusinessException("Нельзя записаться на нерабочий день.");
        var interval = TimelineBuilder.BuildInterval(startTime, appointmentOfferingData);
        EnsureAppointmentInterval(interval, checkPast: checkPast);
        var appointment = Appointment.Create(this, userId, appointmentOfferingData, interval);
        _appointments.Add(appointment);
        return appointment;
    }

    public void AttachAppointment(Appointment appointment)
    {
        if (!IsWorking)
            throw new BusinessException("Нельзя записаться на нерабочий день.");
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
        ScheduleRules.EnsureBreakInsideWorkInterval(WorkInterval, newBreakInterval);
        ScheduleRules.EnsureBreakDoesNotTouchBorders(WorkInterval, newBreakInterval);
        if (newBreakInterval is not null && _appointments.Any(a =>
                a.Interval.IsOverlapping(newBreakInterval) &&
                IsActive(a)))
            throw new BusinessException("Заданное время перерыва пересекается с назначенными записями.");
        BreakInterval = newBreakInterval;
    }

    public void ChangeWorkInterval(TimeInterval newWorkInterval, bool checkPast = false)
    {
        if (checkPast && DateTime.Now > Date.ToDateTime(newWorkInterval.Start))
            throw new BusinessException("Нельзя создать рабочий день в прошлом.");
        ScheduleRules.EnsureWorkIntervalDuration(newWorkInterval);
        ScheduleRules.EnsureBreakInsideWorkInterval(newWorkInterval, BreakInterval);
        if (_appointments.Any(a =>
                !a.Interval.IsInside(newWorkInterval) &&
                IsActive(a)))
            throw new BusinessException("Назначенные записи не входят в заданное рабочее время.");
        WorkInterval = newWorkInterval;
    }

    public void ChangeIsWorking(bool isWorking)
    {
        if (!isWorking && HasActiveAppointments)
            throw new BusinessException("Нельзя завершить рабочий день с назначенными записями.");
        IsWorking = isWorking;
    }

    /// <summary>
    /// Приводит день к настройкам генерации и помечает его как Auto.
    /// Дни с активными записями вызывающий пропускает.
    /// </summary>
    public void AlignToSettings(bool isWorking, TimeInterval workInterval, TimeInterval? breakInterval)
    {
        if (BreakInterval is not null && !BreakInterval.IsInside(workInterval))
            ChangeBreakInterval(null);
        ChangeWorkInterval(workInterval);
        ChangeIsWorking(isWorking);
        ChangeBreakInterval(breakInterval);
        Source = ScheduleSource.Auto;
    }

    /// <summary>
    /// Помечает день как измененный вручную: генерация его не трогает.
    /// </summary>
    public void MarkAsManual()
    {
        Source = ScheduleSource.Manual;
    }
}
