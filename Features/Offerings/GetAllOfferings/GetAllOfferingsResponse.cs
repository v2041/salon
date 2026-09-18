using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Offerings.GetAllOfferings;

public record GetAllOfferingsResponse(
    Guid Id,
    string Title,
    string Description,
    Money Price,
    TimeSpan Duration,
    Category Category
);
