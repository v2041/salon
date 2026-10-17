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
        var appointments = await db.Appointments
            .AsNoTracking()
            .Include(a => a.Schedule)
            .Include(a => a.Offerings)
            .OrderBy(a => a.Schedule.Date)
            .ThenBy(a => a.Interval.Start)
            .ToListAsync(token);

        var userIds = appointments.Select(a => a.UserId).Distinct().ToList();
        var users = await db.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new UserShort(u.Id, u.FirstName, u.LastName, u.Phone))
            .ToDictionaryAsync(u => u.Id, token);

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
            .Select(a =>
            {
                var user = users.GetValueOrDefault(a.UserId);
                return new GetAllAppointmentsResponse(
                    a.Id,
                    a.Schedule.Date,
                    a.Interval,
                    a.Price,
                    a.Offerings
                        .Select(o => new AdminAppointmentOfferingResponse(
                            o.OfferingId,
                            titles.GetValueOrDefault(o.OfferingId, "Услуга"),
                            o.Price,
                            o.Duration))
                        .ToList(),
                    a.UserId,
                    user is null ? "Клиент" : $"{user.FirstName} {user.LastName}".Trim(),
                    user?.Phone ?? "",
                    a.Status
                );
            })
            .ToList();

        return Results.Ok(response);
    }

    private record UserShort(Guid Id, string FirstName, string? LastName, string Phone);
}
