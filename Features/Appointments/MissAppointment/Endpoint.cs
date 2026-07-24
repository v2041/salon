using FluentValidation;
using Infrastructure.Data;

namespace Features.Appointments.MissAppointment;

public class Endpoint
{
    public static async Task<IResult> MissAppointmentAsync(
        [AsParameters] MissAppointmentRequest request,
        IValidator<MissAppointmentRequest> validator,
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
        appointment.Miss();
        await db.SaveChangesAsync(token);
        var response = new MissAppointmentResponse(appointment.Id, appointment.Date, appointment.Interval, appointment.Price, appointment.Status);
        return Results.Ok(response);
    }
}