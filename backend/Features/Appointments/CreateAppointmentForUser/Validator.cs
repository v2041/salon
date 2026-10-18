using FluentValidation;

namespace Features.Appointments.CreateAppointmentForUser;

public class Validator : AbstractValidator<CreateAppointmentForUserRequest>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Телефон обязателен");

        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .WithMessage("Максимальная длина имени - 50 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.OfferingIds)
            .NotEmpty()
            .WithMessage("Список услуг обязателен")
            .Must(ids => ids != null && ids.Distinct().Count() == ids.Count)
            .WithMessage("Список указанных услуг содержит дубликаты");
    }
}
