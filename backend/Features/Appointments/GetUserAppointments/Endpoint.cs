using System.Security.Claims;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.GetUserAppointments;

public static class Endpoint
{
    public static async Task<IResult> GetUserAppointmentsAsync(
        [AsParameters] GetUserAppointmentsRequest request,
        IValidator<GetUserAppointmentsRequest> validator,
        HttpContext context,
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

        var value = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value == null) return Results.NotFound();
        var userId = Guid.Parse(value);

        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);
        var upcoming = request.Scope == "upcoming";

        var slice = db.Appointments
            .AsNoTracking()
            .Where(a => a.UserId == userId);

        slice = upcoming
            ? slice.Where(Appointment.IsUpcoming(today, nowTime))
            : slice.Where(Appointment.IsPast(today, nowTime));

        var totalCount = await slice.CountAsync(token);

        var appointments = await slice
            .OrderByScope(upcoming)
            .Include(a => a.Schedule)
            .Include(a => a.Offerings)
            .TakePage(request.Page, request.PageSize)
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

        return Results.Ok(new GetUserAppointmentsPageResponse(
            response,
            request.Page,
            request.PageSize,
            totalCount));
    }
}
