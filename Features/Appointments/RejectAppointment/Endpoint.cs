using FluentValidation;
using Infrastructure.Data;

namespace Features.Appointments.RejectAppointment;

public class Endpoint
{
    public static async Task<IResult> RejectAppointmentAsync(
        [AsParameters] RejectAppointmentRequest request,
        IValidator<RejectAppointmentRequest> validator,
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
        appointment.Reject();
        await db.SaveChangesAsync(token);
        var response = new RejectAppointmentResponse(appointment.Id, appointment.Date, appointment.Interval, appointment.Price, appointment.Status);
        return Results.Ok(response);
    }
}