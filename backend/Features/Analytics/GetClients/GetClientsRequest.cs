using Microsoft.AspNetCore.Mvc;

namespace Features.Analytics.GetClients;

public record GetClientsRequest(
    [FromQuery] DateOnly From,
    [FromQuery] DateOnly To
);
