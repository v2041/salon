using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.GetAllAppointments;

public static class Endpoint
{
    public static async Task<IResult> GetAllAppointmentsAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var response = await db.Appointments
            .AsNoTracking()
            .Select(appointment =>
                new GetAllAppointmentsResponse(
                    appointment.Id,
                    appointment.Schedule.Date,
                    appointment.Interval,
                    appointment.Price,
                    appointment.Offerings.Select(o => o.OfferingId),
                    appointment.UserId,
                    appointment.Status
                )
            )
            .ToListAsync(token);
        return Results.Ok(response);
    }
}