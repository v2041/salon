using FluentValidation;

namespace Features.Users.SendVerificationCode;

public class Validator : AbstractValidator<SendVerificationCodeRequest>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(request => request.Phone)
            .NotEmpty()
            .WithMessage("Требуется номер телефона.")
            .Matches(@"^\+7\d{10}$")
            .WithMessage("Телефон должен быть в формате +7XXXXXXXXXX.");
    }
}
