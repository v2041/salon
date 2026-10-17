using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.GetAppointment;

public static class Endpoint
{
    public static async Task<IResult> GetAppointmentAsync(
        [AsParameters] GetAppointmentRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var appointment = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Schedule)
            .Include(a => a.Offerings)
            .FirstOrDefaultAsync(a => a.Id == request.Id, token);

        if (appointment is null) return Results.NotFound();

        var response = new GetAppointmentResponse(
            appointment.Id,
            appointment.Schedule.Date,
            appointment.Interval,
            appointment.UserId,
            appointment.Status,
            appointment.Price
        );

        return Results.Ok(response);
    }
}