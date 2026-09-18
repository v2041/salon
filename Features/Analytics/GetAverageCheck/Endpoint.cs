using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Analytics.GetAverageCheck;

public static class Endpoint
{
    public static async Task<IResult> GetAverageCheckAsync(
        GetAverageCheckRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var response = new GetAverageCheckResponse(Money.FromDecimal(await db.Appointments
            .AsNoTracking()
            .Where(a => a.Status == AppointmentStatus.Completed && a.Schedule.Date >= request.From &&
                        a.Schedule.Date <= request.To)
            .SelectMany(a => a.Offerings)
            .Select(ao => ao.Price)
            .AverageAsync(m => m.Value, token)
        ));
        return Results.Ok(response);
    }
}