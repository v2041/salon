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
        SalonDbContext db,
        CancellationToken token
    )
    {
        var appointment = await db.Appointments.FirstOrDefaultAsync(a => a.Id == request.Id, token);
        if (appointment is null) return Results.NotFound();
        switch (request.Status)
        {
            case AppointmentStatus.Confirmed:
                appointment.Confirm();
                break;
            case AppointmentStatus.Rejected:
                appointment.Reject();
                break;
            case AppointmentStatus.Canceled:
                appointment.Cancel();
                break;
            case AppointmentStatus.Completed:
                appointment.Complete();
                break;
            case AppointmentStatus.Missed:
                appointment.Miss();
                break;
            default:
                throw new NotFoundException("Статус не найден");
        }

        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}