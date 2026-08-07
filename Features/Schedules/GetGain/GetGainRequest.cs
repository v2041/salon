using Microsoft.AspNetCore.Mvc;

namespace Features.Schedules.GetGain;

public record GetGainRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);