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
        var response = await db.Appointments
            .AsNoTracking()
            .Where(a => a.Id == request.Id)
            .Select(appointment => new GetAppointmentResponse(
                appointment.Id,
                appointment.Schedule.Date,
                appointment.Interval,
                appointment.UserId,
                appointment.Status,
                appointment.Price
            ))
            .FirstOrDefaultAsync(token);
        return response == null ? Results.NotFound() : Results.Ok(response);
    }
}