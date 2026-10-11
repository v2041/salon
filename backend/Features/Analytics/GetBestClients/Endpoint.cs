using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Analytics.GetBestClients;

public static class Endpoint
{
    public static async Task<IResult> GetBestClientsAsync(
        [AsParameters] GetBestClientRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var rows = await db.Users
            .AsNoTracking()
            .Select(u => new
            {
                Name = u.FirstName,
                PhoneNumber = u.Phone,
                Spent = db.Appointments
                    .Where(a => a.UserId == u.Id && a.Status == AppointmentStatus.Completed)
                    .SelectMany(a => a.Offerings)
                    .Sum(ao => (decimal?)ao.Price.Value) ?? 0m
            })
            .Where(x => x.Spent > 0)
            .OrderByDescending(x => x.Spent)
            .ThenBy(x => x.Name)
            .Take(request.Limit)
            .ToListAsync(token);

        var response = rows
            .Select(r =>
                new GetBestClientsResponse(r.Name, r.PhoneNumber, Money.FromDecimal(r.Spent)));
        return Results.Ok(response);
    }
}