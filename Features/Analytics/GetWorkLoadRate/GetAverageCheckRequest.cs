using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetWorkLoadRate;

public record GetWorkLoadRateRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);