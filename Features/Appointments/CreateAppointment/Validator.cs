using FluentValidation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.CreateAppointment;

public class Validator : AbstractValidator<CreateAppointmentRequest>
{
    private readonly SalonDbContext _db;

    public Validator(SalonDbContext db)
    {
        _db = db;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId обязателен")
            .MustAsync(async (userId, token) =>
                await _db.Users.AnyAsync(x => x.Id == userId, token))
            .WithMessage("Пользователь не найден");

        RuleFor(x => x.OfferingIds)
            .NotEmpty()
            .WithMessage("OfferingsId обязателен")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Список указанных услуг содержит дубликаты")
            .MustAsync(async (ids, token) =>
            {
                var existingCount = await _db.Offerings.CountAsync(x => ids.Contains(x.Id), token);
                return existingCount == ids.Count;
            })
            .WithMessage("Одна или несколько указанных услуг не найдены");


        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Дата обязательна")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Дата не может быть в прошлом")
            .MustAsync(async (x, token) =>
                await _db.Schedules.AnyAsync(s => s.Date == x && s.IsWorking == true, token)
            )
            .WithMessage("Не рабочая дата");

        RuleFor(x => x.Time)
            .NotEmpty()
            .WithMessage("Время обязательно");
    }
}