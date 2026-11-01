using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetGainByMonth;

public record GetGainByMonthRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);
