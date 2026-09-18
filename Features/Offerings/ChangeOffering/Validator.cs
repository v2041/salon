using Domain.Entities;
using FluentValidation;

namespace Features.Offerings.ChangeOffering;

public class Validator : AbstractValidator<ChangeOfferingRequest>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Требуется Id.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Название не должно быть пустым.")
            .MaximumLength(Offering.MAX_TITLE_LENGTH)
            .WithMessage("Название слишком длинное.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Описание не должно быть пустым.");

        RuleFor(x => x.Duration)
            .GreaterThan(TimeSpan.FromMinutes(Offering.MIN_DURATION_MINUTES))
            .WithMessage($"Минимальная длительность услуги - {Offering.MIN_DURATION_MINUTES} минут.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Цена не должна быть отрицательной.");
    }
}
