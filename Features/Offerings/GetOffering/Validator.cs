using FluentValidation;
using Infrastructure.Data;

namespace Features.Offerings.GetOffering;

public class Validator : AbstractValidator<GetOfferingRequest>
{
    public Validator()
    {

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Требуется Id.");

    }
}