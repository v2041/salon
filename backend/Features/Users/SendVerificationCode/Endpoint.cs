using Domain.Entities;
using FluentValidation;
using Infrastructure.Data;
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

        var phone = VerificationCode.NormalizePhone(request.Phone);
        var isRegistered = await db.Users.AnyAsync(u => u.Phone == phone, token);
        var existing = await db.VerificationCodes.FirstOrDefaultAsync(x => x.Phone == phone, token);

        VerificationCode code;
        if (existing is not null)
        {
            if (!existing.RegenerateCode())
                return Results.Ok(CreateResponse(
                    existing, isRegistered, SendVerificationCodeResponse.StatusSent, null, environment));
            code = existing;
        }
        else
        {
            code = VerificationCode.Create(phone);
            db.VerificationCodes.Add(code);
        }

        var result = await sender.SendAsync(phone, code.Code, token);
        if (!result.Delivered)
        {
            return Results.Ok(CreateResponse(
                code, isRegistered, SendVerificationCodeResponse.StatusBindingRequired, result.BindingUrl,
                environment));
        }

        await db.SaveChangesAsync(token);
        return Results.Ok(CreateResponse(
            code, isRegistered, SendVerificationCodeResponse.StatusSent, null, environment));
    }

    private static SendVerificationCodeResponse CreateResponse(
        VerificationCode code,
        bool isRegistered,
        string status,
        string? bindingUrl,
        IHostEnvironment environment
    ) =>
        new(
            code.ExpiresAt,
            isRegistered,
            status,
            bindingUrl,
            environment.IsDevelopment() ? code.Code : null
        );
}