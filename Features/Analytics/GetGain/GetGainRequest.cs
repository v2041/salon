using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetGain;

public record GetGainRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);