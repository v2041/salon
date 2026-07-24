using FluentValidation;
using Infrastructure.Data;

namespace Features.Schedules.DeleteSchedule;

public class Endpoint
{
    public static async Task<IResult> DeleteScheduleAsync(
        [AsParameters] DeleteScheduleRequest request,
        IValidator<DeleteScheduleRequest> validator,
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

        var schedule = await db.Schedules.FindAsync(request.Id, token);
        db.Remove(schedule);
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}