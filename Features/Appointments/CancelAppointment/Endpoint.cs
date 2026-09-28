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
        SalonDbContext db,
        CancellationToken token
    )
    {
        var value = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value == null) return Results.NotFound();
        var userId = Guid.Parse(value);
        var appointment = await db.Appointments.FirstOrDefaultAsync(a => a.Id == request.Id && a.UserId == userId, token);
        if (appointment is null) return Results.NotFound();
        appointment.Cancel();
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}