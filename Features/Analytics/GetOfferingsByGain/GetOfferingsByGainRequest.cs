using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetOfferingsByGain;

public record GetOfferingsByGainRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);