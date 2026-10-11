using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Offerings.ChangeOffering;

public record ChangeOfferingResponse(
    Guid Id,
    string Title,
    string Description,
    Money Price,
    TimeSpan Duration,
    Category Category
);
