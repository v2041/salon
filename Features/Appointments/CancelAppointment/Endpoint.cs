using FluentValidation;
using Infrastructure.Data;

namespace Features.Appointments.CancelAppointment;

public class Endpoint
{
    public static async Task<IResult> CancelAppointmentAsync(
        [AsParameters] CancelAppointmentRequest request,
        IValidator<CancelAppointmentRequest> validator,
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
        appointment.Cancel();
        await db.SaveChangesAsync(token);
        var response = new CancelAppointmentResponse(appointment.Id, appointment.Date, appointment.Interval, appointment.Price, appointment.Status);
        return Results.Ok(response);
    }
}