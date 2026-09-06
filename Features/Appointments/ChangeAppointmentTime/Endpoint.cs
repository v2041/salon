using Domain.Exceptions;
using Features.Appointments.ChangeAppointment;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.ChangeAppointmentTime;

public static class Endpoint
{
    public static async Task<IResult> ChangeAppointmentAsync(
        ChangeAppointmentTimeRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var appointment = await db.Appointments
            .Include(a => a.Schedule)
            .FirstOrDefaultAsync(a => a.Id == request.Id, token);
        if (appointment == null) throw new NotFoundException("Запись не найдена");
        
        if (request.Time != null)
            appointment.Schedule.MoveAppointment(request.Id, request.Time.Value);

        var newSchedule = await db.Schedules.FirstOrDefaultAsync(s => s.Date == request.Date, token);
        if (newSchedule != null)
            appointment.ChangeSchedule(newSchedule);

        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}