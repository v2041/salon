using FluentValidation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.CreateSchedule;

public class Validator : AbstractValidator<CreateScheduleRequest>
{
    private readonly SalonDbContext _db;

    public Validator(SalonDbContext db)
    {
        _db = db;

        RuleFor(request => request.Date)
            .MustAsync(async (date, token) =>
                !await _db.Schedules.AnyAsync(s => s.Date == date, token)
            )
            .WithMessage("Рабочий день уже назначен на эту дату");
    }
}