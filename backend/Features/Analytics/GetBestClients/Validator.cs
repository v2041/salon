using FluentValidation;

namespace Features.Analytics.GetBestClients;

public class Validator : AbstractValidator<GetBestClientsRequest>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Limit)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Лимит должен быть не меньше 1.");
    }
}
