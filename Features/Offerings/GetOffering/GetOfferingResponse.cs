using Domain.ValueObjects;

namespace Features.Offerings.GetOffering;

public record GetOfferingResponse(
    Guid Id,
    string Title,
    string Description,
    Money Price,
    TimeSpan Duration
    );