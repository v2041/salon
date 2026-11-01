using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetAverageHourGain;

public record GetAverageHourGainRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);