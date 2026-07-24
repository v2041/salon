using Features.Offerings.GetOffering;
using FluentValidation;
using Infrastructure.Data;

namespace Features.Offerings.DeleteOffering;

public class Validator : AbstractValidator<DeleteOfferingRequest>
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
                await _db.Offerings.FindAsync(id, token) != null
            )
            .WithMessage("Услуга не найдена");
    }
}