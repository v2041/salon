
using FluentValidation;
using Infrastructure.Data;

namespace Features.Users.GetUserAppointments;

public class Validator : AbstractValidator<GetUserAppointmentsRequest>
{
    private readonly SalonDbContext _db;

    public Validator(SalonDbContext db)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        _db = db;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Требуется Id.")
            .MustAsync(async (id, token) =>
                await _db.Appointments.FindAsync(id, token) != null
            )
            .WithMessage("Запись не найдена");
    }
}