using Domain.Enums;

namespace Features.Offerings.ChangeOffering;

public record ChangeOfferingRequest(
    Guid Id,
    decimal? Price,
    string? Title,
    string? Description,
    TimeSpan? Duration,
    Category? Category
);
