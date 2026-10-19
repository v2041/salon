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
        var appointment = sourceSchedule.DetachAppointment(appointmentId);
        
        if (newStartTime.HasValue)
        {
            var newInterval = TimeInterval.Create(
                newStartTime.Value, 
                newStartTime.Value.Add(appointment.Interval.Duration));
                
            appointment.ChangeInterval(newInterval);
        }
        
        targetSchedule.AttachAppointment(appointment);
    }
}