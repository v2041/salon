using Domain.ValueObjects;

namespace Features.Analytics.GetBestClients;

public record GetBestClientsResponse(
    string Name,
    string PhoneNumber,
    Money Spent
);