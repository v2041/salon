using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Features.Appointments;

public static class AppointmentCreator
{
    public static async Task<Appointment> CreateAsync(
        SalonDbContext db,
        Schedule schedule,
        Guid userId,
        TimeOnly time,
        IReadOnlyList<Guid> offeringIds,
        CancellationToken token
    )
    {
        var offerings = await db.Offerings
            .AsNoTracking()
            .Where(o => offeringIds.Contains(o.Id))
            .ToListAsync(token);

        if (offerings.Count != offeringIds.Count)
            throw new NotFoundException("Одна или несколько услуг не найдены");
        if (offerings.Any(o => !o.IsActive))
            throw new BusinessException("Одна или несколько услуг недоступны для записи");

        var data = offerings
            .Select(o => new AppointmentOfferingData(o.Id, o.Price, o.Duration))
            .ToList();

        var appointment = schedule.AddAppointment(userId, time, data);

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

        return appointment;
    }

    private static bool IsSlotConflict(DbUpdateException exception)
        => exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
