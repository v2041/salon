using Features.Offerings.GetOffering;
using FluentValidation;
using Infrastructure.Data;

namespace Features.Offerings.DeleteOffering;

public class Validator : AbstractValidator<DeleteOfferingRequest>
{

    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Требуется Id.");
    }
}