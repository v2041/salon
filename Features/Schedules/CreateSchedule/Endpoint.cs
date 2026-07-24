using Domain.Entities;
using Domain.Exceptions;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Features.Schedules.CreateSchedule;

public class Endpoint
{
    public static async Task<IResult> CreateScheduleAsync(
        [FromBody] CreateScheduleRequest request,
        IValidator<CreateScheduleRequest> validator,
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

        try
        {
            var schedule = Schedule.Create(request.Date, true, request.WorkInterval, request.BreakInterval);
            db.Schedules.Add(schedule);
            await db.SaveChangesAsync(token);
            var response = new CreateScheduleResponse(
                schedule.Id,
                schedule.Date,
                schedule.WorkInterval,
                schedule.BreakInterval);
            return Results.Created($"api/schedules/{schedule.Id}", response);
        }
        catch (BusinessException exception)
        {
            return Results.BadRequest(exception.Message);
        }
    }
}