using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Analytics.GetAverageCheck;

public static class Endpoint
{
    public static async Task<IResult> GetAverageCheckAsync(
        [AsParameters] GetAverageCheckRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var average = await db.Appointments
            .AsNoTracking()
            .Where(a => a.Status == AppointmentStatus.Completed && a.Schedule.Date >= request.From &&
                        a.Schedule.Date <= request.To)
            .Select(a => (decimal?)a.Offerings.Sum(o => o.Price.Value))
            .AverageAsync(token) ?? 0m;
        var response = new GetAverageCheckResponse(Money.FromDecimal(average));
        return Results.Ok(response);
    }
}