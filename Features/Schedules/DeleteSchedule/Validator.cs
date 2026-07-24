
using FluentValidation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Schedules.DeleteSchedule;

public class Validator : AbstractValidator<DeleteScheduleRequest>
{
    private readonly SalonDbContext _db;

    public Validator(SalonDbContext db)
    {
        _db = db;

        RuleFor(request => request.Id)
            .MustAsync(async (id, token) =>
                await _db.Schedules.AnyAsync(s => s.Id == id, token)
            )
            .WithMessage("Рабочий день не найден");
    }
}