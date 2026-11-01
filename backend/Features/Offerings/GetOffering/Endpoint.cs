using Domain.Exceptions;
using Features.Common;
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
            return validationResult.ToValidationProblem();

        var response = await db.Offerings
            .AsNoTracking()
            .Where(o => o.Id == request.Id && o.IsActive)
            .Select(o => new GetOfferingResponse(
                o.Id,
                o.Title,
                o.Description,
                o.Price,
                o.Duration,
                o.Category
            ))
            .FirstOrDefaultAsync(token);
        if (response == null)
            throw new NotFoundException("Услуга не найдена");
        return Results.Ok(response);
    }
}
