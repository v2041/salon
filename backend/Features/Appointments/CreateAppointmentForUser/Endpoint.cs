using Domain.Entities;
using Domain.Exceptions;
using Features.Common;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.CreateAppointmentForUser;

public static class Endpoint
{
    public static async Task<IResult> CreateAppointmentForUserAsync(
        [FromBody] CreateAppointmentForUserRequest request,
        IValidator<CreateAppointmentForUserRequest> validator,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var validationResult = await validator.ValidateAsync(request, token);
        if (!validationResult.IsValid)
            return validationResult.ToValidationProblem();

        var phone = request.Phone.Trim();
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Phone == phone, token);
        if (user == null)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName))
                throw new NotFoundException("Клиент не зарегистрирован");
            var lastName = string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName.Trim();
            var created = User.Create(request.FirstName.Trim(), lastName, phone, DateTime.UtcNow);
            db.Users.Add(created);
            user = created;
        }

        var schedule = await db.Schedules
            .Include(s => s.Appointments)
            .FirstOrDefaultAsync(s => s.Date == request.Date, token);
        if (schedule == null)
            throw new NotFoundException("Рабочий день не найден");

        var appointment = await AppointmentCreator.CreateAsync(
            db,
            schedule,
            user.Id,
            request.Time,
            request.OfferingIds,
            token);

        var response = new CreateAppointment.CreateAppointmentResponse(
            appointment.Id,
            appointment.Schedule.Date,
            appointment.Interval,
            appointment.Status,
            appointment.Price
        );
        return Results.Created($"api/appointments/{appointment.Id}", response);
    }
}
