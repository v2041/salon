using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Services;

public static class AppointmentTimeChanger
{
    public static void Reschedule(
        Schedule sourceSchedule,
        Schedule targetSchedule,
        Guid appointmentId,
        TimeOnly? newStartTime)
    {
        var appointment = sourceSchedule.Appointments.FirstOrDefault(a => a.Id == appointmentId);
        if (appointment is null)
            throw new BusinessException("Запись не найдена");

        var newInterval = newStartTime.HasValue
            ? TimeInterval.Create(newStartTime.Value, appointment.Interval.Duration)
            : appointment.Interval;

        if (!targetSchedule.IsWorking)
            throw new BusinessException("Нельзя записаться на нерабочий день.");
        targetSchedule.EnsureAppointmentInterval(newInterval, appointment.Id);

        sourceSchedule.DetachAppointment(appointmentId);
        appointment.ChangeInterval(newInterval);
        targetSchedule.AttachAppointment(appointment);
    }
}
