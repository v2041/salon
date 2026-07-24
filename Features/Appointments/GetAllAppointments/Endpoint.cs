using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.GetAllAppointments;

public class Endpoint
{
    public static async Task<IResult> GetAllAppointmentsAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var appointments = await db.Appointments.ToListAsync(token);
        var response = new List<GetAllAppointmentsResponse>();
        foreach (var appointment in appointments)
        {
            response.Add(new GetAllAppointmentsResponse(
                appointment.Id,
                appointment.Date,
                appointment.Interval,
                appointment.UserId,
                appointment.Status
            ));
        }

        return Results.Ok(response);
    }
}