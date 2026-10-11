using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetOfferingsByPopularity;

public record GetOfferingsByPopularityRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);