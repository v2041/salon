using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.ChangeAppointmentStatus;

public static class Endpoint
{
    public static async Task<IResult> ChangeAppointmentStatusAsync(
        [FromBody] ChangeAppointmentStatusRequest request,
        TimeProvider timeProvider,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var appointment = await db.Appointments
            .Include(a => a.Schedule)
            .FirstOrDefaultAsync(a => a.Id == request.Id, token);
        if (appointment is null)
            throw new NotFoundException("Запись не найдена");
        switch (request.Status)
        {
            case AppointmentStatus.Pending:
                throw new BusinessException("Нельзя изменить статус на Pending");
            case AppointmentStatus.Confirmed:
                appointment.Confirm();
                break;
            case AppointmentStatus.Rejected:
                appointment.Reject();
                break;
            case AppointmentStatus.Canceled:
                appointment.Cancel(timeProvider);
                break;
            case AppointmentStatus.Completed:
                appointment.Complete();
                break;
            case AppointmentStatus.Missed:
                appointment.Miss();
                break;
            default:
                throw new BusinessException("Статус не найден");
        }
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}