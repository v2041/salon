using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Analytics.GetClients;

public static class Endpoint
{
    public static async Task<IResult> GetClientsAsync(
        [AsParameters] GetClientsRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var periodStart = request.From.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var periodEnd = request.To.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var clientIds = await db.Appointments
            .AsNoTracking()
            .Where(a => a.Status == AppointmentStatus.Completed &&
                        a.Schedule.Date >= request.From && a.Schedule.Date <= request.To)
            .Select(a => a.UserId)
            .Distinct()
            .ToListAsync(token);

        var newClientIds = await db.Appointments
            .AsNoTracking()
            .Where(a => clientIds.Contains(a.UserId))
            .GroupBy(a => a.UserId)
            .Where(g => g.Min(a => a.CreatedAt) >= periodStart && g.Min(a => a.CreatedAt) < periodEnd)
            .Select(g => g.Key)
            .ToListAsync(token);

        var total = clientIds.Count;
        var newClients = newClientIds.Count;
        var response = new GetClientsResponse(total, newClients, total - newClients);
        return Results.Ok(response);
    }
}
