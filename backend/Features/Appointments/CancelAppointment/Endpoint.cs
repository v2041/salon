using System.Security.Claims;
using Domain.Enums;
using Domain.Exceptions;
using Features.Appointments.ChangeAppointmentStatus;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.CancelAppointment;

public static class Endpoint
{
    public static async Task<IResult> CancelAppointmentAsync(
        [FromBody] ChangeAppointmentStatusRequest request,
        HttpContext context,
        TimeProvider timeProvider,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var value = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value == null)
            throw new NotFoundException("Пользователь не найден");
        var userId = Guid.Parse(value);
        var appointment = await db.Appointments
            .Include(a => a.Schedule)
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.UserId == userId, token);
        if (appointment is null)
            throw new NotFoundException("Запись не найдена");
        appointment.Cancel(timeProvider);
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}