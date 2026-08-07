using FluentValidation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Offerings.GetOffering;

public static class Endpoint
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
        
        var response = await db.Offerings
            .AsNoTracking()
            .Where(o => o.Id == request.Id)
            .Select(o => new GetOfferingResponse(
                o.Id,
                o.Title,
                o.Description,
                o.Price,
                o.Duration
            ))
            .FirstOrDefaultAsync(token);
        return response == null ? Results.NotFound() : Results.Ok(response);
    }
}