using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.CreateAppointment;

public class Endpoint
{
    public static async Task<IResult> CreateAppointmentAsync(
        [FromBody] CreateAppointmentRequest request,
        IValidator<CreateAppointmentRequest> validator,
        SalonDbContext db,
        CancellationToken token)
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

        var offerings = await db.Offerings
            .Where(o => request.OfferingIds!.Contains(o.Id))
            .ToListAsync(token);
        var duration = TimeSpan.Zero;
        foreach (var offering in offerings)
        {
            duration += offering.Duration;
        }


        var schedule = await db.Schedules.FirstAsync(s => s.Date == request.Date && s.IsWorking == true, token);
        var appointments = await db.Appointments.Where(a => a.Date == request.Date).ToListAsync(token);

        var userId = request.UserId;
        var date = request.Date;
        var interval = TimeInterval.Create(request.Time, request.Time.Add(duration));
        var price = offerings.Sum(o => o.Price);
        var workInterval = schedule.WorkInterval;
        var busyIntervals = new List<TimeInterval>();
        busyIntervals.Add(schedule.BreakInterval);
        busyIntervals.AddRange(appointments
            .Where(a => a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending)
            .Select(a => a.Interval));
        var appointment = Appointment.Create(
            userId,
            date,
            interval,
            price,
            workInterval,
            busyIntervals
        );
        foreach (var offering in offerings)
        {
            appointment.AddOffering(offering);
        }

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync(token);

        var response = new CreateAppointmentResponse(
            appointment.Id,
            appointment.Date,
            appointment.Interval,
            appointment.Price,
            appointment.Status);
        return Results.Created($"api/appointments/{appointment.Id}", response);
    }
}
