using FluentValidation;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Users.SendVerificationCode;

public static class Endpoint
{
    public static async Task<IResult> SendVerificationCodeAsync(
        [FromBody] SendVerificationCodeRequest request,
        IValidator<SendVerificationCodeRequest> validator,
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

        var isRegistered = await db.Users.AnyAsync(u => u.Phone == request.Phone, token);
        var existing = await db.VerificationCodes.FirstOrDefaultAsync(x => x.Phone == request.Phone, token);
        VerificationCode code;
        if (existing is not null)
        {
            existing.RegenerateCode();
            code = existing;
        }
        else
        {
            code = VerificationCode.Create(request.Phone);
            db.VerificationCodes.Add(code);
        }
        await db.SaveChangesAsync(token);
        var response = new SendVerificationCodeResponse(code.ExpiresAt, isRegistered);
        return Results.Ok(response);
    }
}