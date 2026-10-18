using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.ChangeAppointmentOfferingDuration;

public static class Endpoint
{
    public static async Task<IResult> ChangeAppointmentOfferingDurationAsync(
        [AsParameters] ChangeAppointmentOfferingDurationRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var appointment = await db.Appointments
            .Include(a => a.Offerings)
            .Include(a => a.Schedule)
            .ThenInclude(s => s.Appointments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, token);
        if (appointment == null)
            throw new NotFoundException("Запись не найдена");

        var appointmentOffering = appointment.Offerings.FirstOrDefault(ao => ao.OfferingId == request.OfferingId);
        if (appointmentOffering == null)
            throw new NotFoundException("Услуга не найдена");

        appointment.ChangeOfferingDuration(request.OfferingId, request.Duration);
        await db.SaveChangesAsync(token);

        return Results.NoContent();
    }
}
