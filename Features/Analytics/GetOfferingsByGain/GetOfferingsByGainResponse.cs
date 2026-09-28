using Domain.ValueObjects;

namespace Features.Analytics.GetOfferingsByGain;

public record GetOfferingsByGainResponse(
    string Name,
    int Count,
    Money Price
);