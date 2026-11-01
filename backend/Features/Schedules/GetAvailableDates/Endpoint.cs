using Domain.Exceptions;
using Domain.Services;
using Features.Common;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.GetAvailableDates;

public static class Endpoint
{
    public static async Task<IResult> GetAvailableDatesAsync(
        [FromQuery(Name = "offeringIds")] GetAvailableDatesRequest request,
        IValidator<GetAvailableDatesRequest> validator,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var validationResult = await validator.ValidateAsync(request, token);
        if (!validationResult.IsValid)
            return validationResult.ToValidationProblem();

        var offerings = await db.Offerings
            .AsNoTracking()
            .Where(o => request.OfferingIds.Contains(o.Id) && o.IsActive)
            .Select(o => new { o.Id, o.Duration })
            .ToListAsync(token);
        if (offerings.Count != request.OfferingIds.Count)
            throw new NotFoundException("Одна или несколько услуг не найдены");
        var duration = offerings.Aggregate(TimeSpan.Zero, (current, o) => current + o.Duration);

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