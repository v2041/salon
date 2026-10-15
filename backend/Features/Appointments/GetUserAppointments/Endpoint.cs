using System.Security.Claims;
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

        var appointments = await db.Appointments
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Include(a => a.Schedule)
            .Include(a => a.Offerings)
            .OrderBy(a => a.Schedule.Date)
            .ThenBy(a => a.Interval.Start)
            .ToListAsync(token);

        var offeringIds = appointments
            .SelectMany(a => a.Offerings)
            .Select(o => o.OfferingId)
            .Distinct()
            .ToList();

        var titles = await db.Offerings
            .AsNoTracking()
            .Where(o => offeringIds.Contains(o.Id))
            .ToDictionaryAsync(o => o.Id, o => o.Title, token);

        var response = appointments
            .Select(a => new GetUserAppointmentsResponse(
                a.Id,
                a.Schedule.Date,
                a.Interval,
                a.Status,
                a.Price,
                a.Offerings
                    .Select(o => new UserAppointmentOfferingResponse(
                        titles.GetValueOrDefault(o.OfferingId, "Услуга"),
                        o.Price))
                    .ToList()
            ))
            .ToList();

        return Results.Ok(response);
    }
}
