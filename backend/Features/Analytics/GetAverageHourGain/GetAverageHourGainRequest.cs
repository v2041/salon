using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.AverageHourGain;

public record GetAverageHourGainRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);