using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetBestClients;

public record GetBestClientsRequest(
    [FromQuery] int Limit
);