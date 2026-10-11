using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetNoShowRate;

public record GetNoShowRateRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);