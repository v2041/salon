using System.Security.Claims;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.GetUserAppointments;

public static class Endpoint
{
    public static async Task<IResult> GetUserAppointmentsAsync(
        HttpContext context,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var value = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value == null) return Results.NotFound();
        var userId = Guid.Parse(value);
        var response = await db.Appointments
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Include(a => a.Offerings)
            .Select(a => new GetUserAppointmentsResponse(
                a.Id,
                a.Schedule.Date,
                a.Interval,
                a.Status,
                a.Price
            ))
            .ToListAsync(token);

        return Results.Ok(response);
    }
}