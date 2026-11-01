using System.Security.Claims;
using Domain.Exceptions;
using Domain.Services;
using Domain.ValueObjects;
using Features.Common;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

        var offerings = await db.Offerings
            .AsNoTracking()
            .Where(o => request.OfferingIds.Contains(o.Id))
            .ToListAsync(token);
        
        if (offerings.Count != request.OfferingIds.Count)
            throw new NotFoundException("Одна или несколько услуг не найдены");
        if (offerings.Any(o => !o.IsActive))
            throw new BusinessException("Одна или несколько услуг недоступны для записи");

        var appointmentOfferingDataList = offerings
            .Select(o => new AppointmentOfferingData(o.Id, o.Price, o.Duration))
            .ToList();
        
        var appointment = schedule.AddAppointment(userId, request.Time, appointmentOfferingDataList);

        await using var transaction = await db.Database.BeginTransactionAsync(token);
        try
        {
            await db.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
        }
        catch (DbUpdateException exception) when (IsSlotConflict(exception))
        {
            throw new BusinessException("Время уже занято, выберите другое");
        }

        var response = new CreateAppointmentResponse(
            appointment.Id,
            appointment.Schedule.Date,
            appointment.Interval,
            appointment.Status,
            appointment.Price
        );
        return Results.Created($"api/appointments/{appointment.Id}", response);
    }

    private static bool IsSlotConflict(DbUpdateException exception)
        => exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}