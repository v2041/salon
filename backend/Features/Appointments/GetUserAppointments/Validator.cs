using FluentValidation;

namespace Features.Appointments.GetUserAppointments;

public class Validator : AbstractValidator<GetUserAppointmentsRequest>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Scope)
            .NotEmpty().WithMessage("Параметр scope обязателен")
            .Must(scope => scope is "upcoming" or "past")
            .WithMessage("Параметр scope принимает значения upcoming и past");

        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Параметр page должен быть не меньше 1");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Параметр pageSize должен быть от 1 до 100");
    }
}
