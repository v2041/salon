using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.GetAllAppointments;

public static class Endpoint
{
    public static async Task<IResult> GetAllAppointmentsAsync(
        [AsParameters] GetAllAppointmentsRequest request,
        IValidator<GetAllAppointmentsRequest> validator,
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

        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);
        var upcoming = request.Scope == "upcoming";

        var slice = db.Appointments
            .AsNoTracking();

        if (request.Status.HasValue)
        {
            var status = (AppointmentStatus)request.Status.Value;
            slice = slice.Where(a => a.Status == status);
        }

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

        return Results.Ok(new GetAllAppointmentsPageResponse(
            response,
            request.Page,
            request.PageSize,
            totalCount));
    }

    private record UserShort(Guid Id, string FirstName, string? LastName, string Phone);
}
