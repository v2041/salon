using FluentValidation;

namespace Features.Analytics.GetBestClients;

public class Validator : AbstractValidator<GetBestClientRequest>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Limit)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Лимит должен быть не меньше 1.");
    }
}
