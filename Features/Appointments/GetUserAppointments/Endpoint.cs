using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.GetUserAppointments;

public static class Endpoint
{
    public static async Task<IResult> GetUserAppointmentsAsync(
        [AsParameters] GetUserAppointmentsRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        if (!await db.Users.AnyAsync(u => u.Id == request.UserId, token))
            return Results.NotFound();
        var response = await db.Appointments
            .AsNoTracking()
            .Where(a => a.UserId == request.UserId)
            .Include(a => a.Offerings)
            .Select(a => new GetUserAppointmentsResponse(
                a.Id,
                a.Schedule.Date,
                a.Interval,
                a.UserId,
                a.Status,
                a.Price
            ))
            .ToListAsync(token);

        return Results.Ok(response);
    }
}