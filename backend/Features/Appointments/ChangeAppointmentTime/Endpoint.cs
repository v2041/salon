using Domain.Entities;
using Domain.Exceptions;
using Domain.Services;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.ChangeAppointmentTime;

public static class Endpoint
{
    public static async Task<IResult> ChangeAppointmentTimeAsync(
        ChangeAppointmentTimeRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var appointment = await db.Appointments
            .Include(a => a.Offerings)
            .Include(a => a.Schedule)
            .ThenInclude(s => s.Appointments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == request.Id, token);
        if (appointment == null) throw new NotFoundException("Запись не найдена");
        if (appointment.Schedule.Date == request.Date)
        {
            appointment.Schedule.MoveAppointment(appointment.Id, request.Time);
        }
        else
        {
            var newSchedule = await db.Schedules
                .Include(s => s.Appointments)
                .FirstOrDefaultAsync(s => s.Date == request.Date, token);
            if (newSchedule == null) throw new NotFoundException("Рабочий день не найден");

            AppointmentTimeChanger.Reschedule(appointment.Schedule, newSchedule, appointment.Id, request.Time);
        }

        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}