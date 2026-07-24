
using Features.Appointments.GetAppointment;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Features.Appointments.GetAppointment;


public class Endpoint
{
    public static async Task<IResult> GetAppointmentAsync(
        [AsParameters] GetAppointmentRequest request,
        IValidator<GetAppointmentRequest> validator,
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
        var appointment = await db.Appointments.FindAsync(request.Id, token);
        var response = new GetAppointmentResponse(
            appointment.Id,
            appointment.Date,
            appointment.Interval,
            appointment.UserId,
            appointment.Status
        );
        return Results.Ok(response);
    }
}