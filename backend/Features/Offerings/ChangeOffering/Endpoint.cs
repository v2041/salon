using Domain.Exceptions;
using Domain.ValueObjects;
using Features.Common;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Offerings.ChangeOffering;

public static class Endpoint
{
    public static async Task<IResult> ChangeOfferingAsync(
        [AsParameters] ChangeOfferingRequest request,
        IValidator<ChangeOfferingRequest> validator,
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

        if (request.Price is not null)
            offering.ChangePrice(Money.FromDecimal(request.Price.Value));
        if (request.Title is not null)
            offering.ChangeTitle(request.Title);
        if (request.Description is not null)
            offering.ChangeDescription(request.Description);
        if (request.Duration is not null)
            offering.ChangeDuration(request.Duration.Value);
        if (request.Category is not null)
            offering.ChangeCategory(request.Category.Value);

        await db.SaveChangesAsync(token);

        var response = new ChangeOfferingResponse(
            offering.Id,
            offering.Title,
            offering.Description,
            offering.Price,
            offering.Duration,
            offering.Category
        );
        return Results.Ok(response);
    }
}
