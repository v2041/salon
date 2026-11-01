using System.Security.Claims;
using Domain.Exceptions;
using Domain.Services;
using Features.Common;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.CreateAppointment;

public static class Endpoint
{
    public static async Task<IResult> CreateAppointmentAsync(
        [FromBody] CreateAppointmentRequest request,
        HttpContext context,
        IValidator<CreateAppointmentRequest> validator,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var validationResult = await validator.ValidateAsync(request, token);
        if (!validationResult.IsValid)
            return validationResult.ToValidationProblem();

        var value = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value == null)
            throw new NotFoundException("Пользователь не найден");
        var userId = Guid.Parse(value);

        var schedule = await db.Schedules
            .Include(s => s.Appointments)
            .FirstOrDefaultAsync(s => s.Date == request.Date, token);
        if (schedule == null)
            throw new NotFoundException("Рабочий день не найден");

        var appointment = await AppointmentCreator.CreateAsync(
            db,
            schedule,
            userId,
            request.Time,
            request.OfferingIds,
            token);

        var response = new CreateAppointmentResponse(
            appointment.Id,
            appointment.Schedule.Date,
            appointment.Interval,
            appointment.Status,
            appointment.Price
        );
        return Results.Created($"api/appointments/{appointment.Id}", response);
    }
}