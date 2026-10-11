using FluentValidation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Offerings.DeleteOffering;

public static class Endpoint
{
    public static async Task<IResult> DeleteOfferingAsync(
        [AsParameters] DeleteOfferingRequest request,
        IValidator<DeleteOfferingRequest> validator,
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
        var offering = await db.Offerings.FirstOrDefaultAsync(o => o.Id == request.Id, token);
        if (offering == null) return Results.NotFound();
        offering.Remove();
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}