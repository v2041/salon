using Domain.Services;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.GetAvailableDates;

public static class Endpoint
{
    public static async Task<IResult> GetAvailableDatesAsync(
        [FromQuery] GetAvailableDatesRequest request,
        IValidator<GetAvailableDatesRequest> validator,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var validationResult = await validator.ValidateAsync(request, token);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            return Results.ValidationProblem(errors);
        }

        var durations = await db.Offerings
            .AsNoTracking()
            .Where(o => request.OfferingIds.Contains(o.Id))
            .Distinct()
            .Select(o => o.Duration)
            .ToListAsync(token);
        if (durations.Count != request.OfferingIds.Count) return Results.NotFound();
        var duration = durations.Aggregate(TimeSpan.Zero, (current, s) => current + s);
        var schedules = await db.Schedules
            .Include(s => s.Appointments)
            .AsNoTracking()
            .Where(s => s.Date >= DateOnly.FromDateTime(DateTime.Now))
            .ToListAsync(token);

        var response = new GetAvailableDatesResponse(schedules
            .Where(s => TimelineBuilder.FindFreeIntervals(s).Any(i => i.Duration >= duration))
            .Select(s => s.Date)
            .ToList()
        );


        return Results.Ok(response);
    }
}