using FluentValidation;

namespace Features.Offerings.CreateOffering;

public class Validator : AbstractValidator<CreateOfferingRequest>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
            
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Требуется название.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Требуется описание.");

        RuleFor(x => x.Duration)
            .NotEmpty()
            .WithMessage("Требуется длительность.")
            .GreaterThan(TimeSpan.Zero);
    }
}