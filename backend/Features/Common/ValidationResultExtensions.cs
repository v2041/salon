using FluentValidation.Results;

namespace Features.Common;

public static class ValidationResultExtensions
{
    public static IResult ToValidationProblem(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
        return Results.ValidationProblem(errors);
    }
}
