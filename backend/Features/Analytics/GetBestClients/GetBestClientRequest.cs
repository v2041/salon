using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetBestClients;

public record GetBestClientRequest(
    [FromQuery] int Limit
);