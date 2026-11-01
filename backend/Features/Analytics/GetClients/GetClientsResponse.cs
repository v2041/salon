namespace Features.Analytics.GetClients;

public record GetClientsResponse(
    int Total,
    int New,
    int Returning
);
