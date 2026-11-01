using FluentValidation;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Users.SendVerificationCode;

public static class Endpoint
{
    public static async Task<IResult> SendVerificationCodeAsync(
        [FromBody] SendVerificationCodeRequest request,
        IValidator<SendVerificationCodeRequest> validator,
        SalonDbContext db,
        ICodeSender sender,
        IHostEnvironment environment,
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
            if (!existing.RegenerateCode())
                return Results.Ok(CreateSentResponse(existing, isRegistered, environment));
            code = existing;
        }
        else
        {
            code = VerificationCode.Create(request.Phone);
            db.VerificationCodes.Add(code);
        }

        var result = await sender.SendAsync(request.Phone, code.Code, token);
        if (!result.Delivered)
        {
            return Results.Ok(new SendVerificationCodeResponse(
                code.ExpiresAt,
                isRegistered,
                SendVerificationCodeResponse.StatusBindingRequired,
                result.BindingUrl,
                environment.IsDevelopment() ? code.Code : null
            ));
        }

        await db.SaveChangesAsync(token);
        return Results.Ok(CreateSentResponse(code, isRegistered, environment));
    }

    private static SendVerificationCodeResponse CreateSentResponse(
        VerificationCode code,
        bool isRegistered,
        IHostEnvironment environment
    ) =>
        new(
            code.ExpiresAt,
            isRegistered,
            SendVerificationCodeResponse.StatusSent,
            null,
            environment.IsDevelopment() ? code.Code : null
        );
}
