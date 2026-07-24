using FluentValidation;

namespace Features.Offerings.CreateOffering;

public class Validator : AbstractValidator<CreateOfferingRequest>
{
    public Validator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Требуется название.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Требуется описание.");

        RuleFor(x => x.Price)
            .NotEmpty()
            .WithMessage("Требуется цена.")
            .GreaterThan(0)
            .WithMessage("Цена должна быть больше 0.");
    }
}