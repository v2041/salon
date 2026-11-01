using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Offerings.GetAllOfferingsAdmin;

public record GetAllOfferingsAdminResponse(
    Guid Id,
    string Title,
    string Description,
    Money Price,
    TimeSpan Duration,
    Category Category,
    bool IsActive
);
