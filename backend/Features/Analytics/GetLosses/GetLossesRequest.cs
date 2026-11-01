using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetLosses;

public record GetLossesRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);
