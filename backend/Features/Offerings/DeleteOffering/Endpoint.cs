using Domain.Exceptions;
using Features.Common;
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
            return validationResult.ToValidationProblem();
        var offering = await db.Offerings.FirstOrDefaultAsync(o => o.Id == request.Id, token);
        if (offering == null)
            throw new NotFoundException("Услуга не найдена");
        offering.Remove();
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}