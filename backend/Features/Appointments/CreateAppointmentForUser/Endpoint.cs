using Domain.Entities;
using Domain.Exceptions;
using Domain.Services;
using Domain.ValueObjects;
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
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            return Results.ValidationProblem(errors);
        }

        var phone = request.Phone.Trim();
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Phone == phone, token);
        if (user == null)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName))
                throw new NotFoundException("Клиент не зарегистрирован");
            var created = User.Create(request.FirstName.Trim(), request.LastName?.Trim(), phone, DateTime.UtcNow);
            db.Users.Add(created);
            user = created;
        }

        var schedule = await db.Schedules
            .Include(s => s.Appointments)
            .FirstOrDefaultAsync(s => s.Date == request.Date, token);
        if (schedule == null)
            throw new NotFoundException("Рабочий день не найден");

        var offerings = await db.Offerings
            .AsNoTracking()
            .Where(o => request.OfferingIds.Contains(o.Id))
            .ToListAsync(token);

        if (offerings.Count != request.OfferingIds.Count)
            throw new NotFoundException("Одна или несколько услуг не найдены");

        var appointmentOfferingDataList = offerings
            .Select(o => new AppointmentOfferingData(o.Id, o.Price, o.Duration))
            .ToList();

        var appointment = schedule.AddAppointment(user.Id, request.Time, appointmentOfferingDataList);

        await db.SaveChangesAsync(token);

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
