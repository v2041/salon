using Features.Appointments.CancelAppointment;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Features.Offerings.GetOffering;

public class Endpoint
{
    public static async Task<IResult> GetOfferingAsync(
        [AsParameters] GetOfferingRequest request,
        IValidator<GetOfferingRequest> validator,
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

        var offering = await db.Offerings.FindAsync(request.Id, token);

        var response = new GetOfferingResponse(
            offering.Id,
            offering.Title,
            offering.Description,
            offering.Price,
            offering.Duration
        );
        return Results.Ok(response);
    }
}