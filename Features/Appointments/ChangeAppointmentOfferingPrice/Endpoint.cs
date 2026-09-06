using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.ChangeAppointmentOffering;

public static class Endpoint
{
    public static async Task<IResult> ChangeAppointmentOfferingPriceAsync(
        ChangeAppointmentOfferingPriceRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var appointment = await db.Appointments
            .Include(a => a.Offerings)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, token);
        if (appointment == null)
            throw new NotFoundException("Запись не найдена");

        var appointmentOffering = appointment.Offerings.FirstOrDefault(ao => ao.OfferingId == request.OfferingId);
        if (appointmentOffering == null)
            throw new NotFoundException("Услуга не найдена");

        appointment.ChangeOfferingPrice(request.OfferingId, request.Price);
        await db.SaveChangesAsync(token);

        return Results.NoContent();
    }
}