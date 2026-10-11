using FluentValidation;

namespace Features.Schedules.GetAvailableDates;

public class Validator : AbstractValidator<GetAvailableDatesRequest>
{
    public Validator()
    {
        RuleFor(request => request.OfferingIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Список услуг содержит дубликаты");
    }
}