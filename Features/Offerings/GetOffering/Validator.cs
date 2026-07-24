using FluentValidation;
using Infrastructure.Data;

namespace Features.Offerings.GetOffering;

public class Validator : AbstractValidator<GetOfferingRequest>
{
    private readonly SalonDbContext _db;

    public Validator(SalonDbContext db)
    {
        _db = db;
        RuleLevelCascadeMode = CascadeMode.Stop;
        
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Требуется Id.")
            .MustAsync(async (id, token) =>
                await _db.Appointments.FindAsync(id, token) != null
            )
            .WithMessage("Услуга не найдена");
    }
}