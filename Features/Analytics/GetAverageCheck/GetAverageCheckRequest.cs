using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetAverageCheck;

public record GetAverageCheckRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);