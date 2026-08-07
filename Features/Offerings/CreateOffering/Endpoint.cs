using Domain.Entities;
using Domain.ValueObjects;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Offerings.CreateOffering;

public static class Endpoint
{
    public static async Task<IResult> CreateOfferingAsync(
        [FromBody] CreateOfferingRequest request,
        IValidator<CreateOfferingRequest> validator,
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

        if (await db.Offerings.AnyAsync(o => EF.Functions.ILike(o.Title, request.Title), token))
            return Results.Conflict();

        var offering = Offering.Create(Money.FromDecimal(request.Price), request.Title, request.Description,
            request.Duration);
        db.Offerings.Add(offering);
        await db.SaveChangesAsync(token);
        var response = new CreateOfferingResponse(offering.Id, offering.Title, offering.Description, offering.Price,
            offering.Duration);
        return Results.Created($"api/offerings/{offering.Id}", response);
    }
}