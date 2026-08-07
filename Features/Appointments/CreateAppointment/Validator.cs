using FluentValidation;

namespace Features.Appointments.CreateAppointment;

public class Validator : AbstractValidator<CreateAppointmentRequest>
{

    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId обязателен");

        RuleFor(x => x.OfferingIds)
            .NotEmpty()
            .WithMessage("Список услуг обязателен")
            .Must(ids => ids != null && ids.Distinct().Count() == ids.Count)
            .WithMessage("Список указанных услуг содержит дубликаты");
    }
}