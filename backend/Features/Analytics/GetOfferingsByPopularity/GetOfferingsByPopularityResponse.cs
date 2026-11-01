using Domain.ValueObjects;

namespace Features.Analytics.GetOfferingsByPopularity;

public record GetOfferingsByPopularityResponse(
    string Name,
    int Count,
    Money Price
);